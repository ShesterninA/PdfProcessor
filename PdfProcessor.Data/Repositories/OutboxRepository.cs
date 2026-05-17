using Microsoft.EntityFrameworkCore;
using PdfProcessor.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PdfProcessor.Data.Repositories
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly PdfProcessorDbContext _dbContext;

        public OutboxRepository(PdfProcessorDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(OutboxMessage message) => _dbContext.OutboxMessages.Add(message);

        public async Task<List<OutboxMessage>> GetUnprocessedAsync(int takeCount)
        {
            return await _dbContext.OutboxMessages
                .Where(x => x.ProcessedAt == null)
                .OrderBy(x => x.CreatedAt)
                .Take(takeCount)
                .ToListAsync();
        }

        public void Update(OutboxMessage message)
        {
            _dbContext.OutboxMessages.Update(message);
        }
    }
}
