using System;

namespace recycling.Model
{
    /// <summary>
    /// 操作结果类 - 用于统一封装业务操作的返回结果
    /// 可替代 (bool Success, string Message) 元组，提供更好的类型安全性和可扩展性
    /// </summary>
    public class OperationResult
    {
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 操作消息（成功或失败的描述信息）
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 错误代码（可选，用于前端国际化或特定错误处理）
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// 创建成功的操作结果
        /// </summary>
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

        /// <summary>
        /// 创建失败的操作结果
        /// </summary>
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

    /// <summary>
    /// 带数据的操作结果类 - 用于返回操作结果和数据
    /// </summary>
    /// <typeparam name="T">返回数据的类型</typeparam>
    public class OperationResult<T> : OperationResult
    {
        /// <summary>
        /// 操作返回的数据
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 创建带数据的成功结果
        /// </summary>
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

        /// <summary>
        /// 创建带数据的失败结果
        /// </summary>
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
