using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Friends_of_Award_FS_Lib.Services
{
    public class VotingConfigService
    {
        private readonly DateTime _votingEndUtc;

        public VotingConfigService(IConfiguration config)
        {
            _votingEndUtc = DateTime.Parse(config["VotingSettings:VotingEndUtc"]);
        }

        public bool IsVotingOpen()
        {
            return DateTime.UtcNow <= _votingEndUtc;
        }

        public DateTime VotingEndUtc => _votingEndUtc;
    }
}
