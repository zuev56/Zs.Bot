using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zs.Common.Services.Abstractions
{
    public interface ISeqService
    {
        Task<List<string>> GetLastEvents(int take = 10, params int[] signals);
        Task<List<string>> GetLastEvents(DateTime fromDate, int take = 10, params int[] signals);
    }
}