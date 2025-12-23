/**
 * Keyboard Navigation Utility
 * 键盘导航工具
 * 
 * 为表单输入框添加上下箭头键导航功能
 * Adds up/down arrow key navigation functionality to form input fields
 * 
 * 使用方法 (Usage):
 * KeyboardNavigation.init(); // 在页面加载后调用 (Call after page load)
 * 
 * 功能 (Features):
 * - 上箭头键 (Arrow Up): 移动到上一个输入框
 * - 下箭头键 (Arrow Down): 移动到下一个输入框
 * - Enter键 (Enter): 移动到下一个输入框，最后一个输入框时提交表单
 * - 自动跳过禁用和隐藏的输入框
 */
(function(window) {
    'use strict';

    // 常量定义 (Constants)
    var FOCUS_DELAY_MS = 100; // 自动聚焦延迟，确保页面完全加载

    var KeyboardNavigation = {
        // 可导航的输入框选择器
        inputSelector: 'input[type="text"], input[type="password"], input[type="email"], input[type="tel"], input[type="number"], textarea, select',
        
        /**
         * 初始化键盘导航
         * @param {string} containerSelector - 可选的容器选择器，限制导航范围
         */
        init: function(containerSelector) {
            var self = this;
            var container = containerSelector ? document.querySelector(containerSelector) : document;
            
            if (!container) {
                console.warn('KeyboardNavigation: Container not found / 容器未找到');
                return;
            }

            // 获取所有可导航的输入框
            var inputs = container.querySelectorAll(this.inputSelector);
            
            if (inputs.length === 0) {
                return;
            }

            // 为每个输入框添加键盘事件监听
            inputs.forEach(function(input, index) {
                input.addEventListener('keydown', function(e) {
                    self.handleKeyDown(e, inputs, index);
                });
            });

            // 自动聚焦到第一个输入框
            this.focusFirstInput(inputs);
        },

        /**
         * 处理键盘按键事件
         * @param {KeyboardEvent} e - 键盘事件
         * @param {NodeList} inputs - 所有输入框列表
         * @param {number} currentIndex - 当前输入框索引
         */
        handleKeyDown: function(e, inputs, currentIndex) {
            var key = e.key || e.keyCode;
            
            // 上箭头键 - 移动到上一个输入框
            if (key === 'ArrowUp' || key === 38) {
                e.preventDefault();
                this.moveToPrevious(inputs, currentIndex);
            }
            // 下箭头键 - 移动到下一个输入框
            else if (key === 'ArrowDown' || key === 40) {
                e.preventDefault();
                this.moveToNext(inputs, currentIndex);
            }
            // Enter键 - 移动到下一个输入框或提交表单
            else if (key === 'Enter' || key === 13) {
                // 对于textarea，允许换行
                if (e.target.tagName.toLowerCase() === 'textarea') {
                    return;
                }
                
                e.preventDefault();
                
                // 如果是最后一个输入框，尝试提交表单
                if (currentIndex === inputs.length - 1) {
                    this.submitForm(e.target);
                } else {
                    this.moveToNext(inputs, currentIndex);
                }
            }
        },

        /**
         * 移动到上一个可用的输入框
         * @param {NodeList} inputs - 所有输入框列表
         * @param {number} currentIndex - 当前输入框索引
         */
        moveToPrevious: function(inputs, currentIndex) {
            for (var i = currentIndex - 1; i >= 0; i--) {
                if (this.isInputAccessible(inputs[i])) {
                    inputs[i].focus();
                    // 对于文本输入框，选中所有文本
                    if (inputs[i].select) {
                        inputs[i].select();
                    }
                    return;
                }
            }
            // 如果没有上一个，回到最后一个
            for (var i = inputs.length - 1; i > currentIndex; i--) {
                if (this.isInputAccessible(inputs[i])) {
                    inputs[i].focus();
                    if (inputs[i].select) {
                        inputs[i].select();
                    }
                    return;
                }
            }
        },

        /**
         * 移动到下一个可用的输入框
         * @param {NodeList} inputs - 所有输入框列表
         * @param {number} currentIndex - 当前输入框索引
         */
        moveToNext: function(inputs, currentIndex) {
            for (var i = currentIndex + 1; i < inputs.length; i++) {
                if (this.isInputAccessible(inputs[i])) {
                    inputs[i].focus();
                    // 对于文本输入框，选中所有文本
                    if (inputs[i].select) {
                        inputs[i].select();
                    }
                    return;
                }
            }
            // 如果没有下一个，回到第一个
            for (var i = 0; i < currentIndex; i++) {
                if (this.isInputAccessible(inputs[i])) {
                    inputs[i].focus();
                    if (inputs[i].select) {
                        inputs[i].select();
                    }
                    return;
                }
            }
        },

        /**
         * 检查输入框是否可访问
         * @param {HTMLElement} input - 输入框元素
         * @returns {boolean} - 是否可访问
         */
        isInputAccessible: function(input) {
            return input && 
                   !input.disabled && 
                   !input.readOnly &&
                   input.offsetParent !== null && // 检查元素是否可见
                   input.type !== 'hidden';
        },

        /**
         * 聚焦到第一个可用的输入框
         * @param {NodeList} inputs - 所有输入框列表
         */
        focusFirstInput: function(inputs) {
            for (var i = 0; i < inputs.length; i++) {
                if (this.isInputAccessible(inputs[i])) {
                    // 使用IIFE捕获当前输入框元素，避免闭包问题
                    // Use IIFE to capture the input element for setTimeout callback
                    (function(input) {
                        setTimeout(function() {
                            input.focus();
                        }, FOCUS_DELAY_MS);
                    })(inputs[i]);
                    return;
                }
            }
        },

        /**
         * 提交表单
         * @param {HTMLElement} input - 当前输入框元素
         */
        submitForm: function(input) {
            // input.form 适用于大多数表单关联的输入元素
            // input.closest('form') 作为备用方案，用于某些特殊情况
            var form = input.form || input.closest('form');
            if (form) {
                // 检查表单是否有提交按钮
                var submitBtn = form.querySelector('button[type="submit"], input[type="submit"]');
                if (submitBtn) {
                    submitBtn.click();
                } else {
                    form.submit();
                }
            }
        }
    };

    // 暴露到全局作用域
    window.KeyboardNavigation = KeyboardNavigation;

})(window);
