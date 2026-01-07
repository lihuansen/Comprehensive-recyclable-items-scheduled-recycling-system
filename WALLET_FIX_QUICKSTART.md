# 钱包系统快速修复指南

## 问题

点击"我的钱包"时出现错误：`对象名 'UserPaymentAccounts' 无效`

## 解决方法

### Windows
```batch
cd Database
SetupWalletTables.bat
```

### Linux/macOS
```bash
cd Database
chmod +x SetupWalletTables.sh
./SetupWalletTables.sh
```

## 详细说明

参见 [FIX_WALLET_ERROR_CN.md](FIX_WALLET_ERROR_CN.md)
