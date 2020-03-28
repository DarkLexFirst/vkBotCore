using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VkBotCore.Callback
{
    /// <summary>
    /// Помечает метод как обработчика Callback событий.
    /// <code>Для работы необходимо включить соответствующий тип события в настройках сообщества</code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class CallbackReceive : Attribute
    {
        /// <summary>
        /// Тип события.
        /// </summary>
        public string Type { get; }
        public CallbackReceive(string type) => Type = type;


        /// <summary>
        /// Событие для подтверждения сервера.
        /// </summary>
        public const string Confirmation = "confirmation";

        /// <summary>
        /// События сообщений.
        /// </summary>
        public static class Message
        {
            /// <summary>
            /// Входящее сообщение.
            /// </summary>
            public const string New = "message_new";

            /// <summary>
            /// Исходящее сообщение.
            /// </summary>
            public const string Reply = "message_reply";

            /// <summary>
            /// Редактирование сообщения.
            /// </summary>
            public const string Edit = "message_edit";

            /// <summary>
            /// Подписка на сообщения от сообщества.
            /// </summary>
            public const string Allow = "message_allow";

            /// <summary>
            /// Запрет сообщений от сообщества.
            /// </summary>
            public const string Deny = "message_deny";

            /// <summary>
            /// Статус набора текста.
            /// </summary>
            public const string TypingState = "message_typing_state";
        }

        /// <summary>
        /// События фотографий.
        /// </summary>
        public static class Photo
        {
            /// <summary>
            /// Добавление фотографии.
            /// </summary>
            public const string New = "photo_new";

            /// <summary>
            /// События комментриев к фотографиям.
            /// </summary>
            public static class Comment
            {
                /// <summary>
                /// Новый комментарий.
                /// </summary>
                public const string New = "photo_comment_new";

                /// <summary>
                /// Редактирование комментария.
                /// </summary>
                public const string Edit = "photo_comment_edit";

                /// <summary>
                /// Восстановление комментария.
                /// </summary>
                public const string Restore = "photo_comment_restore";

                /// <summary>
                /// Удаление комментария.
                /// </summary>
                public const string Delete = "photo_comment_delete";
            }
        }

        /// <summary>
        /// События аудио.
        /// </summary>
        public static class Audio
        {
            /// <summary>
            /// Добавление аудио.
            /// </summary>
            public const string New = "audio_new";
        }

        /// <summary>
        /// События видео.
        /// </summary>
        public static class Video
        {
            /// <summary>
            /// Добавление видео.
            /// </summary>
            public const string New = "video_new";

            /// <summary>
            /// События комментриев к видео.
            /// </summary>
            public static class Comment
            {
                /// <summary>
                /// Новый комментарий.
                /// </summary>
                public const string New = "video_comment_new";

                /// <summary>
                /// Редактирование комментария.
                /// </summary>
                public const string Edit = "video_comment_edit";

                /// <summary>
                /// Восстановление комментария.
                /// </summary>
                public const string Restore = "video_comment_restore";

                /// <summary>
                /// Удаление комментария.
                /// </summary>
                public const string Delete = "video_comment_delete";
            }
        }

        /// <summary>
        /// События записей на стене.
        /// </summary>
        public static class Wall
        {
            /// <summary>
            /// Добавление записи.
            /// </summary>
            public const string PostNew = "wall_post_new";

            /// <summary>
            /// Репост записи из сообщества.
            /// </summary>
            public const string Repost = "wall_repost";

            /// <summary>
            /// События комментриев на стене.
            /// </summary>
            public static class Reply
            {
                /// <summary>
                /// Новый комментарий.
                /// </summary>
                public const string New = "wall_reply_new";

                /// <summary>
                /// Редактирование комментария.
                /// </summary>
                public const string Edit = "wall_reply_edit";

                /// <summary>
                /// Восстановление комментария.
                /// </summary>
                public const string Restore = "wall_reply_restore";

                /// <summary>
                /// Удаление комментария.
                /// </summary>
                public const string Delete = "wall_reply_delete";
            }
        }

        /// <summary>
        /// События обсуждений.
        /// </summary>
        public static class BoardPost
        {
            /// <summary>
            /// Новый комментарий.
            /// </summary>
            public const string New = "board_post_new";

            /// <summary>
            /// Редактирование комментария.
            /// </summary>
            public const string Edit = "board_post_edit";

            /// <summary>
            /// Восстановление комментария.
            /// </summary>
            public const string Restore = "board_post_restore";

            /// <summary>
            /// Удаление комментария.
            /// </summary>
            public const string Delete = "board_post_delete";
        }

        /// <summary>
        /// События товаров.
        /// </summary>
        public static class Market
        {
            /// <summary>
            /// События комментриев к товарам.
            /// </summary>
            public static class Comment
            {
                /// <summary>
                /// Новый комментарий.
                /// </summary>
                public const string New = "market_comment_new";

                /// <summary>
                /// Редактирование комментария.
                /// </summary>
                public const string Edit = "market_comment_edit";

                /// <summary>
                /// Восстановление комментария.
                /// </summary>
                public const string Restore = "market_comment_restore";

                /// <summary>
                /// Удаление комментария.
                /// </summary>
                public const string Delete = "market_comment_delete";
            }
        }

        /// <summary>
        /// События сообщества.
        /// </summary>
        public static class Group
        {
            /// <summary>
            /// Вступление в сообщество.
            /// </summary>
            public const string Join = "group_leave";

            /// <summary>
            /// Выход из сообщества.
            /// </summary>
            public const string Leave = "group_join";

            /// <summary>
            /// Редактирование списка руководителей.
            /// </summary>
            public const string OfficersEdit = "group_officers_edit";

            /// <summary>
            /// Изменение настроек сообщества.
            /// </summary>
            public const string ChangeSettings = "group_change_settings";

            /// <summary>
            /// Изменение главного фото.
            /// </summary>
            public const string ChangePhoto = "group_change_photo";
        }

        /// <summary>
        /// События пользователей.
        /// </summary>
        public static class User
        {
            /// <summary>
            /// Добавление пользователя в чёрный список.
            /// </summary>
            public const string Block = "user_block";

            /// <summary>
            /// Удаление пользователя из чёрного списка.
            /// </summary>
            public const string Unblock = "user_unblock";
        }

        /// <summary>
        /// События опросов.
        /// </summary>
        public static class Poll
        {
            /// <summary>
            /// Добавление голоса в публичном опросе.
            /// </summary>
            public const string VoteNew = "poll_vote_new";
        }

        /// <summary>
        /// События платежей.
        /// </summary>
        public static class VkPay
        {
            /// <summary>
            /// платёж через VK Pay.
            /// </summary>
            public const string Transaction = "vkpay_transaction";
        }

        /// <summary>
        /// События приложений.
        /// </summary>
        public static class App
        {
            /// <summary>
            /// Событие в VK Mini Apps.
            /// </summary>
            public const string Payload = "app_payload";
        }
    }

    //[AttributeUsage(AttributeTargets.Method, Inherited = true)]
    //public class MessageReceive : Attribute
    //{
    //    public string ActionType { get; } 
    //    public MessageReceive(string actionType) => ActionType = actionType;
    //}
}
