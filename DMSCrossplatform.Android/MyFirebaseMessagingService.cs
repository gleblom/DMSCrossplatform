﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Extensions;
using Android.OS;
using Android.Util;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.Work;
using DMSCrossplatform.Infrastructure.Android;
using DMSCrossplatform.Infrastructure.Navigation;
using DMSCrossplatform.Services;
using DMSCrossplatform.ViewModels;
using Firebase.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace DMSCrossplatform.Android;

[Service(
    Name = "com.CompanyName.DMSCrossplatform.MyFirebaseMessagingService",
    Exported = false)]
[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
public class MyFirebaseMessagingService : FirebaseMessagingService, IAndroidGetFcmToken
{
    private const string Tag = "FCM";
    private const string ChannelId = "push_high";
    private const int NotificationId = 1001;
    private const string DiagPrefs = "fcm_diag";
    private static IPushService _pushService;
    private static ISessionService _sessionService;
    
    public async override void OnMessageReceived(RemoteMessage message)
    {
        _pushService = App.Services.GetRequiredService<IPushService>();
        _sessionService = App.Services.GetRequiredService<ISessionService>();
        base.OnMessageReceived(message);

        FcmDiag.Mark(this, "on_message_received", DumpMessageBrief(message));

        try
        {
            var readiness = GetNotificationReadiness(this);
            FcmDiag.Mark(this, "readiness",
                $"enabled={readiness.Enabled}; perm={readiness.PostNotificationsGranted}; channelImp={readiness.ChannelImportance}");

            var id = message.Data.ContainsKey("notification_id") ? message.Data["notification_id"] : string.Empty;

            var notification = await _pushService.GetNotification(int.Parse(id));

            if (notification != null && _sessionService.CurrentUser.UserId != notification.UserId)
            {
                return;
            }

            if (NeedsHeavyProcessing(message))
            {
                EnqueueHeavyWork(message);
                FcmDiag.Mark(this, "enqueued_work", message.MessageId ?? "<no-message-id>");
                return;
            }


            EnsureChannel(this);
            ShowNotification(
                title: message.GetNotification()?.Title ?? "Новое сообщение",
                body: message.GetNotification()?.Body ?? "(без текста)",
                extras: message.Data);
            
            

            FcmDiag.Mark(this, "notification_posted", message.MessageId ?? "<no-message-id>");
        }
        catch (Exception ex)
        {
            Log.Error(Tag, ex.ToString());
            FcmDiag.Mark(this, "on_message_received_exception", ex.ToString());
        }
    }

    public override void OnDeletedMessages()
    {
        base.OnDeletedMessages();
        FcmDiag.Mark(this, "on_deleted_messages", "FCM сообщил о пропущенных сообщениях");
    }

    public async Task<string> GetToken(Context context)
    {
        var token = await FirebaseMessaging.Instance.GetToken();
        return token.ToString();
    }

    private static bool NeedsHeavyProcessing(RemoteMessage message)
    {
        return message.Data.Count > 0 &&
               (message.Data.ContainsKey("imageUrl") ||
                message.Data.ContainsKey("fetchDetails") ||
                message.Data.ContainsKey("heavy") ||
                message.Data.Count > 10);
    }

    private void EnqueueHeavyWork(RemoteMessage message)
    {
        var input = new Data.Builder()
            .PutString("messageId", message.MessageId ?? string.Empty)
            .PutString("from", message.From ?? string.Empty)
            .PutString("title", message.GetNotification()?.Title ?? string.Empty)
            .PutString("body", message.GetNotification()?.Body ?? string.Empty)
            .Build();

        var request = new OneTimeWorkRequest.Builder(typeof(FcmHeavyWorker))
            .SetInputData(input)
            .Build();

        var workName = "fcm_" + (message.MessageId ?? Guid.NewGuid().ToString("N"));
        WorkManager.GetInstance(this)
            .EnqueueUniqueWork(workName, ExistingWorkPolicy.Replace, request);
    }

    private void ShowNotification(string title, string body, IDictionary<string, string> extras)
    {
        EnsureChannel(this);

        var launchIntent = PackageManager.GetLaunchIntentForPackage(PackageName);
        if (launchIntent != null)
        {
            launchIntent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
        }

        var pendingIntent = PendingIntent.GetActivity(
            this,
            0,
            launchIntent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        var builder = new NotificationCompat.Builder(this, ChannelId)
            .SetSmallIcon(global::Android.Resource.Drawable.CheckboxOffBackground) 
            .SetContentTitle(title)
            .SetContentText(body)
            .SetStyle(new NotificationCompat.BigTextStyle().BigText(body))
            .SetAutoCancel(true)
            .SetPriority((int)NotificationPriority.High)
            .SetContentIntent(pendingIntent);

        if (extras.Count > 0)
        {
            var inbox = new NotificationCompat.InboxStyle();
            foreach (var kv in extras)
                inbox.AddLine($"{kv.Key}: {kv.Value}");

            builder.SetStyle(inbox);
        }

        NotificationManagerCompat.From(this).Notify(NotificationId, builder.Build());
    }

    private static void EnsureChannel(Context context)
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            return;

        var nm = (NotificationManager)context.GetSystemService(NotificationService)!;
        var channel = nm.GetNotificationChannel(ChannelId);
        if (channel != null)
            return;

        var created = new NotificationChannel(
            ChannelId,
            "Push notifications",
            NotificationImportance.High)
        {
            Description = "FCM notifications"
        };

        nm.CreateNotificationChannel(created);
    }

    private static NotificationReadiness GetNotificationReadiness(Context context)
    {
        var enabled = NotificationManagerCompat.From(context).AreNotificationsEnabled();

        var permGranted =
            Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu ||
            ContextCompat.CheckSelfPermission(context, Manifest.Permission.PostNotifications) == Permission.Granted;

        int? channelImportance = null;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var nm = (NotificationManager)context.GetSystemService(NotificationService)!;
            var ch = nm.GetNotificationChannel(ChannelId);
            channelImportance = ch?.Importance == NotificationImportance.High ? (int?)NotificationImportance.High : null;
        }

        return new NotificationReadiness(enabled, permGranted, channelImportance);
    }

    private static bool HasNotificationPermission(Context context)
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu)
            return true;

        return ContextCompat.CheckSelfPermission(context, Manifest.Permission.PostNotifications) == Permission.Granted;
    }

    private static string DumpMessageBrief(RemoteMessage message)
    {
        var dataCount = message.Data?.Count ?? 0;
        return $"id={message.MessageId ?? "<null>"}; from={message.From ?? "<null>"}; dataCount={dataCount}; notif={(message.GetNotification() != null ? "yes" : "no")}";
    }

    private readonly record struct NotificationReadiness(bool Enabled, bool PostNotificationsGranted, int? ChannelImportance);
}

public class FcmHeavyWorker : Worker
{
    private const string Tag = "FCM_WORKER";
    private const string ChannelId = "push_high";
    private const int NotificationId = 1002;

    public FcmHeavyWorker(Context context, WorkerParameters workerParams) : base(context, workerParams)
    {
    }

    public override Result DoWork()
    {
        try
        {
            FcmDiag.Mark(ApplicationContext, "worker_started");

            var title = InputData.GetString("title");
            var body = InputData.GetString("body");
            var dataJson = InputData.GetString("dataJson") ?? "{}";



            EnsureChannel(ApplicationContext);
            
            var intent = new Intent(ApplicationContext, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
            intent.PutExtra("DataJson", dataJson);
            
            var pendingIntentFlags = Build.VERSION.SdkInt >= BuildVersionCodes.S 
                ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable 
                : PendingIntentFlags.UpdateCurrent;
            var pendingIntent = PendingIntent.GetActivity(ApplicationContext, 0, intent, pendingIntentFlags);
            

            var notification = new NotificationCompat.Builder(ApplicationContext, ChannelId)
                .SetSmallIcon(global::Android.Resource.Drawable.CheckboxOffBackground)
                .SetContentTitle(string.IsNullOrWhiteSpace(title) ? "Новое сообщение" : title)
                .SetContentText(body)
                .SetContentIntent(pendingIntent)
                .SetStyle(new NotificationCompat.BigTextStyle().BigText(body))
                .SetAutoCancel(true)
                .SetPriority((int)NotificationPriority.High)
                .Build();

            

            NotificationManagerCompat.From(ApplicationContext).Notify(NotificationId, notification);

            FcmDiag.Mark(ApplicationContext, "worker_notification_posted");
            return Result.InvokeSuccess();
        }
        catch (Exception ex)
        {
            Log.Error(Tag, ex.ToString());
            FcmDiag.Mark(ApplicationContext, "worker_exception", ex.ToString());
            return Result.InvokeRetry();
        }
    }
    

    private static void EnsureChannel(Context context)
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            return;

        var nm = (NotificationManager)context.GetSystemService(Context.NotificationService)!;
        if (nm.GetNotificationChannel(ChannelId) != null)
            return;

        nm.CreateNotificationChannel(new NotificationChannel(
            ChannelId,
            "Push notifications",
            NotificationImportance.High));
    }
}

public static class FcmDiag
{
    private const string PrefsName = "fcm_diag";

    public static void Mark(Context context, string stage, string? detail = null)
    {
        var prefs = context.GetSharedPreferences(PrefsName, FileCreationMode.Private);
        prefs.Edit()
            .PutLong("last_ts", Java.Lang.JavaSystem.CurrentTimeMillis())
            .PutString("last_stage", stage)
            .PutString("last_detail", detail ?? string.Empty)
            .Apply();

        Log.Info("FCM_DIAG", $"{stage} | {detail}");
    }
}