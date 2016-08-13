﻿using Newtonsoft.Json;

using SplitBook.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SplitBook.Request
{
    class DeleteFriendRequest : RestBaseRequest
    {
        public static String deleteFriendURL = "delete_friend/";
        private int friendId;

        public DeleteFriendRequest(int id)
            : base()
        {
            friendId = id;
        }

        public async void deleteFriend(Action<bool> CallbackOnSuccess, Action<HttpStatusCode> CallbackOnFailure)
        {
            try
            {
                HttpResponseMessage response = await client.PostAsync(deleteFriendURL + friendId.ToString(), null);
                JsonSerializerSettings settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                DeleteFriend result = Newtonsoft.Json.JsonConvert.DeserializeObject<DeleteFriend>(await response.Content.ReadAsStringAsync(), settings);
                if (result.success)
                    CallbackOnSuccess(result.success);
                else
                    CallbackOnFailure(response.StatusCode);
            }
            catch (Exception e)
            {
                CallbackOnFailure(HttpStatusCode.ServiceUnavailable);
            }
        }

        private class DeleteFriend
        {
            public Boolean success { get; set; }
            public Object error { get; set; }
        }
    }
}
