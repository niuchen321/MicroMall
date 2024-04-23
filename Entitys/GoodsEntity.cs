using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entitys
{
    /// <summary>
    /// 商品
    /// </summary>
    public class GoodsEntity
    {
        /// <summary>
        /// id
        /// </summary>
        public required string Id { get; set; }
        /// <summary>
        /// 类型id
        /// </summary>
        public required string TypeId { get; set; }
        /// <summary>
        /// 类型名称
        /// </summary>
        public required string TypeName { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public float Price { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Number { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        public string? Image { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }
    }
}
