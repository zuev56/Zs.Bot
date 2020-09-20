using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Zs.Bot.Model.Db
{
    3D000: ���� ������ "ZsBot" �� ����������

       at Npgsql.NpgsqlConnector.<>c__DisplayClass160_0.<<DoReadMessage>g__ReadMessageLong|0>d.MoveNext()
    --- End of stack trace from previous location where exception was thrown ---
       at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
       at Npgsql.NpgsqlConnector.<>c__DisplayClass160_0.<<DoReadMessage>g__ReadMessageLong|0>d.MoveNext()
    --- End of stack trace from previous location where exception was thrown ---
       at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
       at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
       at Npgsql.NpgsqlConnector.<Open>d__148.MoveNext()
    --- End of stack trace from previous location where exception was thrown ---
       at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
       at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
       at Npgsql.ConnectorPool.<AllocateLong>d__28.MoveNext()
    --- End of stack trace from previous location where exception was thrown ---
       at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
       at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
       at System.Threading.Tasks.ValueTask`1.get_Result()
       at Npgsql.NpgsqlConnection.<>c__DisplayClass32_0.<<Open>g__OpenLong|0>d.MoveNext()
    --- End of stack trace from previous location where exception was thrown ---
       at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
       at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
       at Npgsql.NpgsqlConnection.Open()
       at Zs.Common.T4.DbReader.GetDbInfo(String connectionString, String[] schemas) in M:\Zs.Bot\Common\Zs.Common.T4\DbReader.cs:line 53
       at Microsoft.VisualStudio.TextTemplatingC4A5F97921713DC26843A7BEB08DB8ED8D06BA130264306560AE44399EE919BD0213CE006EBE3649A7D1C9A89E7D65EE31FCD89E207C50E2FC7BB55B5E661539.GeneratedTextTransformation.TransformText() in M:\Zs.Bot\Bot\Zs.Bot.Model\Model\T4_DbModel.tt:line 27
}