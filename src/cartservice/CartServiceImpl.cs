// Copyright 2018 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using Dynatrace.OneAgent.Sdk;
using Dynatrace.OneAgent.Sdk.Api;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cartservice.interfaces;
using Grpc.Core;
using Hipstershop;
using static Hipstershop.CartService;

namespace cartservice
{
    // Cart wrapper to deal with grpc communication
    internal class CartServiceImpl : CartServiceBase
    {
        private ICartStore cartStore;
        private readonly static Empty Empty = new Empty();

        IOneAgentSdk oneAgentSdk;

        private IIncomingRemoteCallTracer getTracer(Grpc.Core.ServerCallContext context, string methodname){
            
            try {
                Metadata.Entry metadataEntry = context.RequestHeaders.FirstOrDefault(m => String.Equals(m.Key, "x-dynatrace"));

                if (metadataEntry.Equals(default(Metadata.Entry)) || metadataEntry.Value == null) {
                    Console.WriteLine("No x-dynatrace header found in the request.");
                    return null;
                }

                string incomingDynatraceStringTag=metadataEntry.Value;
                IIncomingRemoteCallTracer incomingRemoteCallTracer = oneAgentSdk.TraceIncomingRemoteCall(methodname, "CartService", "grpc://cartservice/"+methodname);
                incomingRemoteCallTracer.SetDynatraceStringTag(incomingDynatraceStringTag);
                incomingRemoteCallTracer.SetProtocolName("gRPC");

                return incomingRemoteCallTracer;
            } catch (Exception e) {
		Console.WriteLine(e.ToString());
		return null;
            }
        }

        public CartServiceImpl(ICartStore cartStore)
        {
            this.cartStore = cartStore;
            oneAgentSdk = OneAgentSdkFactory.CreateInstance();
        }
        public async override Task<Empty> AddItem(AddItemRequest request, Grpc.Core.ServerCallContext context)
        {
            IIncomingRemoteCallTracer incomingRemoteCallTracer = getTracer(context, "AddItem");

            try {
                await incomingRemoteCallTracer.TraceAsync( () => cartStore.AddItemAsync(request.UserId, request.Item.ProductId, request.Item.Quantity));
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                incomingRemoteCallTracer.Error(e);
            } finally {
                incomingRemoteCallTracer.End();
            }

            return Empty;
        }

        public async override Task<Empty> EmptyCart(EmptyCartRequest request, ServerCallContext context)
        {
            IIncomingRemoteCallTracer incomingRemoteCallTracer = getTracer(context, "EmptyCart");

            try {
                await incomingRemoteCallTracer.TraceAsync( () => cartStore.EmptyCartAsync(request.UserId));
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                incomingRemoteCallTracer.Error(e);
            } finally  {
                incomingRemoteCallTracer.End();
            }

            return Empty;
        }

        public override Task<Hipstershop.Cart> GetCart(GetCartRequest request, ServerCallContext context)
        {
            IIncomingRemoteCallTracer incomingRemoteCallTracer = getTracer(context, "GetCart");

            try {
                incomingRemoteCallTracer.Start();
                return cartStore.GetCartAsync(request.UserId);
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                incomingRemoteCallTracer.Error(e);
            } finally {
                incomingRemoteCallTracer.End();
            }
            return null;
        }
    }
}
