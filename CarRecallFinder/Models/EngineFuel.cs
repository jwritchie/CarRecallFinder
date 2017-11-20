using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarRecallFinder.Models
{
    public class EngineFuels
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EngineFuels()
        {
            this.Cars = new HashSet<Car>();
        }

        public int Id { get; set; }
        public string EngineFuelType { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Car> Cars { get; set; }

    }
}