using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFPackager.Services
{
    public class DeployScriptCreator
    {
        public void Do()
        {
            // Write Generic PS Script
                // Connect-SFCluster
                // For each AppType
                    // Test
                    // Copy
                    // Register
                // For each deployed app
                    // Start-Upgrade
        
            // Replace variables with result from packaging
        }
    }
}
