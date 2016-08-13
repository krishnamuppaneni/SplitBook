﻿using SplitBook.Model;
using SplitBook.Request;
using SplitBook.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SplitBook.Controller
{
    public class ModifyDatabase
    {
        Action<bool, HttpStatusCode> callback;

        public ModifyDatabase(Action<bool, HttpStatusCode> callback)
        {
            this.callback = callback;
        }

        public void deleteExpense(int expenseId)
        {
            DeleteExpenseRequest request = new DeleteExpenseRequest(expenseId);
            request.deleteExpense(_OperationSucceded, _OperationFailed);
        }

        public void editExpense(Expense editedExpenseDetail)
        {
            UpdateExpenseRequest request = new UpdateExpenseRequest(editedExpenseDetail);
            request.updateExpense(_OperationSucceded, _OperationFailed);
        }

        public void addExpense(Expense expense)
        {
            AddExpenseRequest request = new AddExpenseRequest(expense);
            request.addExpense(_OperationSucceded, _OperationFailed);
        }

        public void createFriend(string email, string firstName, string lastName)
        {
            CreateFriendRequest request = new CreateFriendRequest(email, firstName, lastName);
            request.createFriend(_FriendAdded, _OperationFailed);
        }

        public void deleteFriend(int friendId)
        {
            DeleteFriendRequest request = new DeleteFriendRequest(friendId);
            request.deleteFriend(_OperationSucceded, _OperationFailed);
        }

        public void createGroup(Group group)
        {
            CreateGroupRequest request = new CreateGroupRequest(group);
            request.createGroup(_GroupAdded, _OperationFailed);
        }

        private void _FriendAdded(User friend)
        {
            //add user to database and to friends list in App.xaml
            //PhoneApplicationService.Current.State[Constants.NEW_USER] = friend;

            using (SplitBookContext db = new SplitBookContext())
            {
                db.Add(friend);                
                friend.picture.user_id = friend.id;
                db.Add(friend.picture);
            }

            callback(true, HttpStatusCode.OK);
        }

        private void _GroupAdded(Group group)
        {
            //add user to database and to friends list in App.xaml
            //PhoneApplicationService.Current.State[Constants.NEW_GROUP] = group;

            using (SplitBookContext db = new SplitBookContext())
            {
                db.Add(group);

                foreach (var debt in group.simplified_debts)
                {
                    debt.group_id = group.id;
                    db.Add(debt);
                }

                foreach (var member in group.members)
                {
                    Group_Members group_member = new Group_Members();
                    group_member.group_id = group.id;
                    group_member.user_id = member.id;
                    db.Add(group_member);
                }

                db.SaveChanges();
            }

            callback(true, HttpStatusCode.OK);
        }


        private void _OperationSucceded(bool status)
        {
            callback(true, HttpStatusCode.OK);
        }

        private void _OperationFailed(HttpStatusCode statusCode)
        {
            callback(false, statusCode);
        }
    }
}
