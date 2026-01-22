using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    public class UserAddressBLL
    {
        private UserAddressDAL _addressDAL = new UserAddressDAL();
        private const int MAX_ADDRESSES = 10; // 每个用户最多10个地址
        private static readonly Regex PhoneRegex = new Regex(@"^1[3-9]\d{9}$", RegexOptions.Compiled);

        /// <summary>
        /// 获取用户的所有地址
        /// </summary>
        public List<UserAddresses> GetUserAddresses(int userId)
        {
            return _addressDAL.GetUserAddresses(userId);
        }

        /// <summary>
        /// 根据ID获取地址
        /// </summary>
        public UserAddresses GetAddressById(int addressId, int userId)
        {
            return _addressDAL.GetAddressById(addressId, userId);
        }

        /// <summary>
        /// 获取用户的默认地址
        /// </summary>
        public UserAddresses GetDefaultAddress(int userId)
        {
            return _addressDAL.GetDefaultAddress(userId);
        }

        /// <summary>
        /// 添加新地址
        /// </summary>
        /// <returns>返回错误信息，null表示成功</returns>
        public (bool Success, string Message, int AddressId) AddAddress(UserAddresses address)
        {
            try
            {
                // 验证地址数量限制
                int currentCount = _addressDAL.GetAddressCount(address.UserID.Value);
                if (currentCount >= MAX_ADDRESSES)
                {
                    return (false, $"最多只能添加{MAX_ADDRESSES}个地址", 0);
                }

                // 验证联系电话格式
                if (!PhoneRegex.IsMatch(address.ContactPhone ?? string.Empty))
                {
                    return (false, "请输入有效的11位手机号码", 0);
                }

                // 验证联系人姓名
                if (string.IsNullOrWhiteSpace(address.ContactName) || address.ContactName.Length > 50)
                {
                    return (false, "联系人姓名不能为空且长度不能超过50个字符", 0);
                }

                // 验证详细地址
                if (string.IsNullOrWhiteSpace(address.DetailAddress) || address.DetailAddress.Length > 200)
                {
                    return (false, "详细地址不能为空且长度不能超过200个字符", 0);
                }

                // 验证街道
                if (string.IsNullOrWhiteSpace(address.Street))
                {
                    return (false, "请选择街道", 0);
                }

                // 设置固定的省市区
                address.Province = "广东省";
                address.City = "深圳市";
                address.District = "罗湖区";
                address.CreatedDate = DateTime.Now;

                // 如果是第一个地址，自动设为默认
                if (currentCount == 0)
                {
                    address.IsDefault = true;
                }

                int newId = _addressDAL.AddAddress(address);
                if (newId > 0)
                {
                    // 如果设置为默认地址，需要取消其他默认
                    if (address.IsDefault == true && currentCount > 0)
                    {
                        _addressDAL.SetDefaultAddress(newId, address.UserID.Value);
                    }
                    return (true, "地址添加成功", newId);
                }
                return (false, "地址添加失败，请重试", 0);
            }
            catch (Exception ex)
            {
                return (false, "添加地址时发生错误：" + ex.Message, 0);
            }
        }

        /// <summary>
        /// 更新地址
        /// </summary>
        public (bool Success, string Message) UpdateAddress(UserAddresses address)
        {
            try
            {
                // 验证地址是否存在且属于该用户
                var existingAddress = _addressDAL.GetAddressById(address.AddressID, address.UserID.Value);
                if (existingAddress == null)
                {
                    return (false, "地址不存在或无权修改");
                }

                // 验证联系电话格式
                if (!PhoneRegex.IsMatch(address.ContactPhone ?? string.Empty))
                {
                    return (false, "请输入有效的11位手机号码");
                }

                // 验证联系人姓名
                if (string.IsNullOrWhiteSpace(address.ContactName) || address.ContactName.Length > 50)
                {
                    return (false, "联系人姓名不能为空且长度不能超过50个字符");
                }

                // 验证详细地址
                if (string.IsNullOrWhiteSpace(address.DetailAddress) || address.DetailAddress.Length > 200)
                {
                    return (false, "详细地址不能为空且长度不能超过200个字符");
                }

                // 验证街道
                if (string.IsNullOrWhiteSpace(address.Street))
                {
                    return (false, "请选择街道");
                }

                bool success = _addressDAL.UpdateAddress(address);
                return success ? (true, "地址更新成功") : (false, "地址更新失败，请重试");
            }
            catch (Exception ex)
            {
                return (false, "更新地址时发生错误：" + ex.Message);
            }
        }

        /// <summary>
        /// 删除地址
        /// </summary>
        public (bool Success, string Message) DeleteAddress(int addressId, int userId)
        {
            try
            {
                // 验证地址是否存在且属于该用户
                var existingAddress = _addressDAL.GetAddressById(addressId, userId);
                if (existingAddress == null)
                {
                    return (false, "地址不存在或无权删除");
                }

                bool wasDefault = existingAddress.IsDefault == true;
                bool success = _addressDAL.DeleteAddress(addressId, userId);
                
                if (success)
                {
                    // 如果删除的是默认地址，需要设置另一个地址为默认
                    if (wasDefault)
                    {
                        var remainingAddresses = _addressDAL.GetUserAddresses(userId);
                        if (remainingAddresses.Count > 0)
                        {
                            _addressDAL.SetDefaultAddress(remainingAddresses[0].AddressID, userId);
                        }
                    }
                    return (true, "地址删除成功");
                }
                return (false, "地址删除失败，请重试");
            }
            catch (Exception ex)
            {
                return (false, "删除地址时发生错误：" + ex.Message);
            }
        }

        /// <summary>
        /// 设置默认地址
        /// </summary>
        public (bool Success, string Message) SetDefaultAddress(int addressId, int userId)
        {
            try
            {
                // 验证地址是否存在且属于该用户
                var existingAddress = _addressDAL.GetAddressById(addressId, userId);
                if (existingAddress == null)
                {
                    return (false, "地址不存在或无权操作");
                }

                // 如果已经是默认地址
                if (existingAddress.IsDefault == true)
                {
                    return (true, "该地址已经是默认地址");
                }

                bool success = _addressDAL.SetDefaultAddress(addressId, userId);
                return success ? (true, "默认地址设置成功") : (false, "设置默认地址失败，请重试");
            }
            catch (Exception ex)
            {
                return (false, "设置默认地址时发生错误：" + ex.Message);
            }
        }

        /// <summary>
        /// 获取用户地址数量
        /// </summary>
        public int GetAddressCount(int userId)
        {
            return _addressDAL.GetAddressCount(userId);
        }
    }
}
