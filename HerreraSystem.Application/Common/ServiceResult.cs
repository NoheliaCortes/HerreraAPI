using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Common
{
    public class ServiceResult<T>
    {
        public bool Success { get; private set; }
        public string? ErrorMessage { get; private set; }
        public T? Data { get; private set; }

        private ServiceResult() { }

        public static ServiceResult<T> Ok(T data) =>
            new() { Success = true, Data = data };

        public static ServiceResult<T> Fail(string errorMessage) =>
            new() { Success = false, ErrorMessage = errorMessage };


    }
}
