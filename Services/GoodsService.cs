using IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class GoodsService : IGoodsService
    {
        public int Price { get; set; }
        public int GetGoods(int id)
        {
            throw new NotImplementedException();
        }

        public int GetGoodsCount()
        {
            throw new NotImplementedException();
        }

        public bool UpdateGoodsService( )
        {
            return false;
        }


    }
}
