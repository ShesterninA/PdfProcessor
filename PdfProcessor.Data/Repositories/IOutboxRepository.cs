using PdfProcessor.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PdfProcessor.Data.Repositories
{
    public interface IOutboxRepository
    {
        void Add(OutboxMessage message);
        Task<List<OutboxMessage>> GetUnprocessedAsync(int takeCount);
        void Update(OutboxMessage message);
    }
}
