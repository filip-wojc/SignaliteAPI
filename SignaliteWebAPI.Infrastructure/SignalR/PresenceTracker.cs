using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace API.SignalR
{
   public class PresenceTracker
   {
      private readonly IConnectionMultiplexer _redis;
      private readonly IDatabase _db;
      private const string UserConnectionPrefix = "user:";
      private const string OnlineUsersKey = "online-users";
      
      public PresenceTracker(IConnectionMultiplexer redis)
      {
         _redis = redis;
         _db = _redis.GetDatabase();
      }

      public async Task<bool> UserConnected(string username, string connectionId)
      {
         var isOnline = false;
         var userKey = $"{UserConnectionPrefix}{username}";
         
         // Add connection ID to user's set
         await _db.SetAddAsync(userKey, connectionId);
         // Check if this is the first connection for the user
         if (await _db.SetLengthAsync(userKey) == 1)
         {
            // First connection, add to online users set
            await _db.SetAddAsync(OnlineUsersKey, username);
            isOnline = true;
         }
         
         return isOnline;
      }

      public async Task<bool> UserDisconnected(string username, string connectionId)
      {
         var isOffline = false;
         var userKey = $"{UserConnectionPrefix}{username}";
         
         // Remove connection ID from user's set
         await _db.SetRemoveAsync(userKey, connectionId);
         
         // Check if user has no more connections
         if (await _db.SetLengthAsync(userKey) == 0)
         {
            // No more connections, remove from online users set
            await _db.KeyDeleteAsync(userKey);
            await _db.SetRemoveAsync(OnlineUsersKey, username);
            isOffline = true;
         }
         
         return isOffline;
      }

      public async Task<string[]> GetOnlineUsers()
      {
         // Get all online users
         var onlineUsers = await _db.SetMembersAsync(OnlineUsersKey);
         return onlineUsers.Select(m => m.ToString()).OrderBy(x => x).ToArray();
      }

      public async Task<List<string>> GetConnectionsForUser(string username)
      {
         var userKey = $"{UserConnectionPrefix}{username}";
         var connections = await _db.SetMembersAsync(userKey);
         
         return connections.Select(c => c.ToString()).ToList();
      }
   }
}