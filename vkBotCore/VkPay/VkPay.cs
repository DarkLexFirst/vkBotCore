namespace VkBotCore.VKPay
{
    public class VkPay
    {
        /// <summary>
        /// Событие платежа.
        /// </summary>
        public VkPayAction Action { get; set; }

        /// <summary>
        /// Тип получателя.
        /// </summary>
        public VkPayTarget Target { get; set; }

        /// <summary>
        /// Сумма платежа.
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// Идентификатор получателя.
        /// </summary>
        public long TargetId { get; set; }

        /// <summary>
        /// Комментарий к платежу.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Идентификатор приложения.
        /// </summary>
        public long AppId { get; set; } = -1;

        public VkPay(VkPayAction action, VkPayTarget target, long targetId, int amount = -1)
        {
            Action = action;
            Target = target;
            Amount = amount;
            TargetId = targetId;
        }

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
