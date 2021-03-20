﻿using System;
using System.Collections.Generic;
using System.Text;
using MethodBoundaryAspect.Fody.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ray.BiliBiliTool.Infrastructure;

namespace Ray.BiliBiliTool.Application.Attributes
{
    /// <summary>
    /// 任务拦截器
    /// </summary>
    public class TaskInterceptorAttribute : OnMethodBoundaryAspect
    {
        private readonly ILogger _logger;
        private readonly string _taskName;
        private readonly TaskLevel _taskLevel;
        private readonly bool _rethrowWhenException;

        public TaskInterceptorAttribute(string taskName = null, TaskLevel taskLevel = TaskLevel.Two, bool rethrowWhenException = true)
        {
            _taskName = taskName;
            _taskLevel = taskLevel;
            _rethrowWhenException = rethrowWhenException;

            _logger = Global.ServiceProviderRoot.GetRequiredService<ILogger<TaskInterceptorAttribute>>();
        }

        public override void OnEntry(MethodExecutionArgs arg)
        {
            if (_taskName == null) return;
            string end = _taskLevel == TaskLevel.One ? "\r\n" : "";
            _logger.LogInformation($"{GetDelimiter()}开始【{_taskName}】{GetDelimiter()}{end}");
        }

        public override void OnExit(MethodExecutionArgs arg)
        {
            if (_taskName == null) return;

            _logger.LogInformation($"{GetDelimiter()}【{_taskName}】结束{GetDelimiter()}\r\n");
        }

        public override void OnException(MethodExecutionArgs arg)
        {
            if (_rethrowWhenException)
            {
                _logger.LogError("程序发生异常：{msg}", arg.Exception.Message);
                base.OnException(arg);
                return;
            }

            _logger.LogError("{task}失败，继续其他任务。失败信息:{msg}\r\n", _taskName, arg.Exception.Message);
            arg.FlowBehavior = FlowBehavior.Continue;
        }

        private string GetDelimiter()
        {
            int count = (int)_taskLevel;
            return new string('-', count);
        }
    }

    public enum TaskLevel
    {
        One = 6,
        Two = 4,
        Three = 2,
    }
}
