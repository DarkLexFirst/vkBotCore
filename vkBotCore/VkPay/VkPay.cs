namespace vkBotCore.VKPay
{
    public class VkPay
    {
        public VkPayAction Action { get; set; }
        public VkPayTarget Target { get; set; }

        public int Amount { get; set; }
        public long TargetId { get; set; }
        public string Description { get; set; }

        public VkPay(VkPayAction action, VkPayTarget target, long targetId, int amount = -1)
        {
            Action = action;
            Target = target;
            Amount = amount;
            TargetId = targetId;
        }

        public long AppId { get; set; } = -1;

        private string GetAction()
        {
            return $"{Action.ToString().ToLower()}-to-{Target.ToString().ToLower()}";
        }

        public override string ToString()
        {
            string hash = $"action={GetAction()}&{Target.ToString().ToLower()}_id={TargetId}";
            if (Amount > 0) hash += $"&amount={Amount}";
            if (!string.IsNullOrEmpty(Description)) hash += $"&description={Description}";
            if (AppId != -1) hash += $"&aid={AppId}";
            return hash;
        }
    }
}
