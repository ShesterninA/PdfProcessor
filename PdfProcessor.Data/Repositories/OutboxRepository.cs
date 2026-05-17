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

        public async Task AddAsync(OutboxMessage message)
        {
            await _dbContext.OutboxMessages.AddAsync(message);
        }
    }
}
