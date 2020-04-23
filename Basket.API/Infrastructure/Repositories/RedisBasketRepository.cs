using Basket.API.Model;
using EasyCaching.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basket.API.Infrastructure.Repositories
{
    public class RedisBasketRepository : IBasketRepository
    {

        private readonly IEasyCachingProviderFactory easyCachingProviderFactory;
        private readonly IEasyCachingProvider easyCachingProvider;

        public RedisBasketRepository(IEasyCachingProviderFactory easyCachingProviderFactory)
        {
            this.easyCachingProviderFactory = easyCachingProviderFactory;
            this.easyCachingProvider = easyCachingProviderFactory.GetCachingProvider("Redis");
        }


        public async Task<bool> DeleteBasketAsync(string id)
        {
            await easyCachingProvider.RemoveAsync("Basket_" + id);
            return true;
        }

        public async Task<CustomerBasket> GetBasketAsync(string customerId)
        {
            var content = await easyCachingProvider.GetAsync<string>(customerId.StartsWith("Basket_") ? customerId : "Basket_" + customerId);

            if (content == null || string.IsNullOrEmpty(content.Value))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<CustomerBasket>(content.Value);
        }

        public IEnumerable<string> GetUsers()
        {
            var data = easyCachingProvider.GetByPrefix<string>("Basket_").Keys.ToList();
            return data;
        }

        public async Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket)
        {
            var content = JsonConvert.SerializeObject(basket);
            await easyCachingProvider.SetAsync("Basket_" + basket.BuyerId, content, TimeSpan.FromDays(1));

            return await GetBasketAsync(basket.BuyerId);

        }
    }
}
