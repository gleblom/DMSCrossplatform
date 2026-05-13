namespace DMSCrossplatform.Pdf.Abstractions;

public interface IPdfDocumentSource
{
    int PageCount { get; }


    (double width, double height) GetPageSize(int pageIndex);
}