using System.ComponentModel;

namespace ProjetoExtensao.Entities.Enums
{
    public enum TemaSistemaEnum : long
    {
        [Description("Claro")]
        Claro = -1,

        [Description("Escuro")]
        Escuro = -2,

        [Description("Padrão do sistema")]
        Padrao = -3
    }
}