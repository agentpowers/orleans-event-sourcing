using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GrainInterfaces
{
    public interface IValueGrain : IGrainWithIntegerKey
    {
        Task<string> GetValue();

        Task SetValue(string value);
    }

    public interface IValueCollectionGrain: IGrainWithStringKey 
    {
        Task<IDictionary<string, string>> GetValues();

        Task Add(string key, string value);

    }
}
