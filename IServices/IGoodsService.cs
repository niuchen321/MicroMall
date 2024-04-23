using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IServices
{
    public interface IGoodsService
    {
        int GetGoodsCount();
        int GetGoods(int id);
    }
}
