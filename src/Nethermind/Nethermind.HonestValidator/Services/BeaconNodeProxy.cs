﻿//  Copyright (c) 2018 Demerzel Solutions Limited
//  This file is part of the Nethermind library.
// 
//  The Nethermind library is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  The Nethermind library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nethermind.BeaconNode;
using Nethermind.BeaconNode.Containers;
using Nethermind.BeaconNode.OApiClient;
using Nethermind.Core2.Crypto;
using Nethermind.Core2.Types;
using BeaconBlock = Nethermind.BeaconNode.Containers.BeaconBlock;
using Fork = Nethermind.Core2.Containers.Fork;
using ValidatorDuty = Nethermind.BeaconNode.ValidatorDuty;

namespace Nethermind.HonestValidator.Services
{
    public class BeaconNodeProxy : IBeaconNodeApi
    {
        private readonly ILogger _logger;
        private readonly BeaconNodeOApiClientFactory _oapiClientFactory;

        public BeaconNodeProxy(ILogger<BeaconNodeProxy> logger, BeaconNodeOApiClientFactory oapiClientFactory)
        {
            _logger = logger;
            _oapiClientFactory = oapiClientFactory;
        }
        
        // The proxy needs to take care of this (i.e. transparent to worker)
        // Not connected: (remote vs local)
        // connect to beacon node (priority order)
        // if not connected, wait and try next

        public async Task<string> GetNodeVersionAsync()
        {
            BeaconNodeOApiClient oapiClient = _oapiClientFactory.CreateClient();
            string result = await oapiClient.VersionAsync().ConfigureAwait(false);
            return result;
        }

        public async Task<ulong> GetGenesisTimeAsync()
        {
            var oapiClient = _oapiClientFactory.CreateClient();
            var result = await oapiClient.TimeAsync().ConfigureAwait(false);
            return result;
        }

        public Task<bool> GetIsSyncingAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<Fork> GetNodeForkAsync()
        {
            throw new System.NotImplementedException();
        }

        public async IAsyncEnumerable<ValidatorDuty> ValidatorDutiesAsync(IEnumerable<BlsPublicKey> validatorPublicKeys, Epoch epoch)
        {
            IEnumerable<byte[]> validator_pubkeys = validatorPublicKeys.Select(x => x.Bytes);
            ulong? epochValue = (epoch != Epoch.None) ? (ulong?) epoch : null; 
            var oapiClient = _oapiClientFactory.CreateClient();
            var result = await oapiClient.DutiesAsync(validator_pubkeys, epochValue).ConfigureAwait(false);
            foreach (var value in result)
            {
                var validatorPublicKey = new BlsPublicKey(value.Validator_pubkey);
                var proposalSlot = value.Block_proposal_slot.HasValue ? new Slot(value.Block_proposal_slot.Value) : Slot.None;
                var validatorDuty = new ValidatorDuty(validatorPublicKey, new Slot(value.Attestation_slot),
                    new Shard(value.Attestation_shard), proposalSlot);
                yield return validatorDuty;
            }
        }

        public Task<BeaconBlock> NewBlockAsync(Slot slot, BlsSignature randaoReveal)
        {
            throw new System.NotImplementedException();
        }
    }
}