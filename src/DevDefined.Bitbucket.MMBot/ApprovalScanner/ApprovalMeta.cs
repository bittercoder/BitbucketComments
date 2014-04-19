using System;
using System.Linq;

namespace DevDefined.Bitbucket.MMBot.ApprovalScanner
{
    public class ApprovalMeta : IEquatable<ApprovalMeta>
    {
        public bool Equals(ApprovalMeta other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Approvers == null && other.Approvers == null) return false;
            if (Approvers == null || other.Approvers == null) return false;
            if (Approvers.Length != other.Approvers.Length) return false;
            var sorted = Approvers.OrderBy(x => x).ToArray();
            var sortedOther = other.Approvers.OrderBy(x => x).ToArray();
            for (int i = 0; i < sorted.Length; i++)
            {
                if (sorted[i] != sortedOther[i]) return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApprovalMeta) obj);
        }

        public override int GetHashCode()
        {
            return (Approvers != null ? Approvers.GetHashCode() : 0);
        }

        public static bool operator ==(ApprovalMeta left, ApprovalMeta right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ApprovalMeta left, ApprovalMeta right)
        {
            return !Equals(left, right);
        }

        public string[] Approvers { get; set; }
    }
}