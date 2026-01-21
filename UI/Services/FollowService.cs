using DataAccessLayer.EntityFramework.Context;
using Entities;
using Microsoft.EntityFrameworkCore;

namespace UI.Services
{
    public class FollowService
    {
        private readonly SafhaDbContext _context;

        public FollowService(SafhaDbContext context)
        {
            _context = context;
        }

        // Kullanıcıyı takip et
        public async Task<bool> FollowUserAsync(int followerId, int followingId)
        {
            if (followerId == followingId)
                return false;

            // Zaten takip ediliyor mu kontrol et
            var existingFollow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId && f.IsActive);

            if (existingFollow != null)
                return false;

            // Yeni takip oluştur
            var follow = new Follow
            {
                FollowerId = followerId,
                FollowingId = followingId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Follows.Add(follow);

            // Takipçi sayılarını güncelle
            var followingUser = await _context.Users.FindAsync(followingId);
            if (followingUser != null)
            {
                followingUser.FollowerCount++;
            }

            var followerUser = await _context.Users.FindAsync(followerId);
            if (followerUser != null)
            {
                followerUser.FollowingCount++;
            }

            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> UnfollowUserAsync(int followerId, int followingId)
        {
            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId && f.IsActive);

            if (follow == null)
                return false;

            follow.IsActive = false;

          
            var followingUser = await _context.Users.FindAsync(followingId);
            if (followingUser != null && followingUser.FollowerCount > 0)
            {
                followingUser.FollowerCount--;
            }

            var followerUser = await _context.Users.FindAsync(followerId);
            if (followerUser != null && followerUser.FollowingCount > 0)
            {
                followerUser.FollowingCount--;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Kullanıcının takipçilerini getir
        public async Task<List<User>> GetFollowersAsync(int userId, int skip = 0, int take = 20)
        {
            return await _context.Follows
                .Where(f => f.FollowingId == userId && f.IsActive)
                .Include(f => f.Follower)
                .OrderByDescending(f => f.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(f => f.Follower)
                .ToListAsync();
        }

        // Kullanıcının takip ettiklerini getir
        public async Task<List<User>> GetFollowingAsync(int userId, int skip = 0, int take = 20)
        {
            return await _context.Follows
                .Where(f => f.FollowerId == userId && f.IsActive)
                .Include(f => f.Following)
                .OrderByDescending(f => f.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(f => f.Following)
                .ToListAsync();
        }

        // Kullanıcı takip ediliyor mu kontrol et
        public async Task<bool> IsFollowingAsync(int followerId, int followingId)
        {
            return await _context.Follows
                .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId && f.IsActive);
        }

        // Takipçi sayısını getir
        public async Task<int> GetFollowerCountAsync(int userId)
        {
            return await _context.Follows
                .CountAsync(f => f.FollowingId == userId && f.IsActive);
        }

        // Takip edilen sayısını getir
        public async Task<int> GetFollowingCountAsync(int userId)
        {
            return await _context.Follows
                .CountAsync(f => f.FollowerId == userId && f.IsActive);
        }
    }
}











