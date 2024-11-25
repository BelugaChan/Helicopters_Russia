using Abstractions.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace Algo.Models
{
    [Table("standarts")]
    public class Standart : IStandart
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("ntd")]
        public string NTD { get; set; }

        [Column("materialntd")]
        public string MaterialNTD { get; set; }

        [Column("ens")]
        public string ENSClassification { get; set; }
    }
}
