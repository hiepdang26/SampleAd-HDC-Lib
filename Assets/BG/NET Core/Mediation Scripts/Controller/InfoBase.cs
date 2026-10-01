using UnityEngine;

namespace BG_Library.NET.Mediation.Base
{
    public class InfoBase
    {
        [SerializeField] protected string id;

        public InfoBase(string id)
        {
            this.id = id;
        }

        public virtual string Id => id;
    }
}