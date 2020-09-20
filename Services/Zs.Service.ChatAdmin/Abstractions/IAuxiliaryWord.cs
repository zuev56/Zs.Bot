using System;

namespace Zs.Service.ChatAdmin.Abstractions
{
    /// <summary> Вспомогательные слова - то, что должно быть отсеяно из статистики </summary>
    public interface IAuxiliaryWord
    {
        string TheWord { get; set; }
        DateTime InsertDate { get; set; }
    }
}
