using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }        // true si todo salió bien, false si hubo error
        public string Message { get; set; } = string.Empty;  // mensaje descriptivo de lo que pasó
        public T? Data { get; set; }             // los datos que retorna el endpoint, null si hay error

        // Método estático para respuestas exitosas — no tienes que instanciar la clase manualmente
        public static ApiResponse<T> Ok(T data, string message = "Operación exitosa") =>
            new() { Success = true, Message = message, Data = data };

        // Método estático para respuestas de error — Data queda null automáticamente
        public static ApiResponse<T> Fail(string message) =>
            new() { Success = false, Message = message, Data = default };
    }
}
