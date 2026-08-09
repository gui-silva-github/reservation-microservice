using Microsoft.EntityFrameworkCore;
using Reservation.SpacesService.DAL.Context;
using Reservation.SpacesService.DAL.Entities;

namespace Reservation.SpacesService.DAL.Repositories
{
    public class SpaceRepository : ISpaceRepository
    {
        private readonly SpacesDbContext _dbContext;

        public SpaceRepository(SpacesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<Space>> GetAllActiveAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Spaces
                .AsNoTracking()
                .Where(space => space.IsActive)
                .OrderBy(space => space.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<Space?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Spaces
                .AsNoTracking()
                .FirstOrDefaultAsync(space => space.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Space>> SearchActiveAsync(string term, CancellationToken cancellationToken = default)
        {
            string normalizedTerm = term.Trim().ToLowerInvariant();

            return await _dbContext.Spaces
                .AsNoTracking()
                .Where(space =>
                        space.IsActive &&
                        (space.Name.ToLower().Contains(normalizedTerm) ||
                        space.Location.ToLower().Contains(normalizedTerm))
                    )
                .OrderBy(space => space.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<Space> AddAsync(Space space, CancellationToken cancellationToken = default)
        {
            _dbContext.Spaces.Add(space);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return space;
        }

        public async Task<Space?> UpdateAsync(Space space, CancellationToken cancellationToken = default)
        {
            Space? existingSpace = await _dbContext.Spaces
                .FirstOrDefaultAsync(entity => entity.Id == space.Id, cancellationToken);

            if (existingSpace is null)
            {
                return null;
            }

            existingSpace.Name = space.Name;
            existingSpace.Location = space.Location;
            existingSpace.Description = space.Description;
            existingSpace.Capacity = space.Capacity;
            existingSpace.PricePerHour = space.PricePerHour;
            existingSpace.IsActive = space.IsActive;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return existingSpace;
        }

        public async Task<bool> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Space? existingSpace = await _dbContext.Spaces
                .FirstOrDefaultAsync(space => space.Id == id, cancellationToken);
            
            if (existingSpace is null)
            {
                return false;
            }

            existingSpace.IsActive = false;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}