using System;
using System.Net;
using System.Diagnostics;

namespace Ressive.Https
{
	
	//
	// SRC: http://www.codeproject.com/Articles/321291/Google-OAuth-on-Windows-Phone
	//
    public class RscHttpsAuthResult
    {
		
        public RscHttpsAuthResult()
        {
            AquiredAt = DateTime.UtcNow;
        }

        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }

        public DateTime AquiredAt { get; set; }

        public DateTime ExpiresAt
        {
            get
            {
                // subtract a minute from the expiration force refresh as we approach expiration
                return AquiredAt + TimeSpan.FromSeconds(expires_in - 60);
            }
        }

        public bool IsExpired
        {
            get
            {
                return DateTime.UtcNow > ExpiresAt;
            }
        }
		
    }
	
}