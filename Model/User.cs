using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotonServerDemo.Model
{
    /// <summary>
    ///mapper the data of database to the class
    ///one to one (class attributes to database col) reflection
    ///2、 attributes need to difine the type of virtual
    /// </summary>
    class User
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual int Age { get; set; }

        public virtual DateTime RecordTime { get; set; }

        public virtual int KillNum { get; set; }
        public virtual int DeathNum { get; set; }


        public override string ToString()
        {
            return $"Id:{Id} Name:{Name} Age:{Age} RecordTime:{RecordTime}";
        }
    }
}
