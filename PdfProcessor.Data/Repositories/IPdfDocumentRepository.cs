using PdfProcessor.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PdfProcessor.Data.Repositories
{
    public interface IPdfDocumentRepository
    {
        Task CreateAsync(PdfDocument document);
    }
}
