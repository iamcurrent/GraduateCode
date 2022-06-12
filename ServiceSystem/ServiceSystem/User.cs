using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceSystem
{
    class User
    {
        public String userName;
        public String publicStr;
        public String flag;
        public User(String userName, String publicStr, String flag)
        {
            this.userName = userName;
            this.publicStr = publicStr;
            this.flag = flag;
        }


    }
}
