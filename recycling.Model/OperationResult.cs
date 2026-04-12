using System;

namespace recycling.Model
{
    /// 操作结果类 - 用于统一封装业务操作的返回结果
    /// 可替代 (bool Success, string Message) 元组，提供更好的类型安全性和可扩展性
    public class OperationResult
    {
        /// 操作是否成功
        public bool Success { get; set; }

        /// 操作消息（成功或失败的描述信息）
        public string Message { get; set; }

        /// 错误代码（可选，用于前端国际化或特定错误处理）
        public string ErrorCode { get; set; }

        /// 创建成功的操作结果
        /// <param name="message">成功消息</param>
        /// <returns>成功的操作结果</returns>
        public static OperationResult CreateSuccess(string message = "操作成功")
        {
            return new OperationResult
            {
                Success = true,
                Message = message
            };
        }

        /// 创建失败的操作结果
        /// <param name="message">失败消息</param>
        /// <param name="errorCode">错误代码（可选）</param>
        /// <returns>失败的操作结果</returns>
        public static OperationResult CreateFailure(string message, string errorCode = null)
        {
            return new OperationResult
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }

    /// 带数据的操作结果类 - 用于返回操作结果和数据
    /// <typeparam name="T">返回数据的类型</typeparam>
    public class OperationResult<T> : OperationResult
    {
        /// 操作返回的数据
        public T Data { get; set; }

        /// 创建带数据的成功结果
        /// <param name="data">返回的数据</param>
        /// <param name="message">成功消息</param>
        /// <returns>成功的操作结果</returns>
        public static OperationResult<T> CreateSuccess(T data, string message = "操作成功")
        {
            return new OperationResult<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        /// 创建带数据的失败结果
        /// <param name="message">失败消息</param>
        /// <param name="errorCode">错误代码（可选）</param>
        /// <returns>失败的操作结果</returns>
        public new static OperationResult<T> CreateFailure(string message, string errorCode = null)
        {
            return new OperationResult<T>
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode,
                Data = default(T)
            };
        }
    }
}
