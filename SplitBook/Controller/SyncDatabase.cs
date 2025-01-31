﻿using SplitBook.Model;
using SplitBook.Request;
using SplitBook.Utilities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SplitBook.Controller
{
    class SyncDatabase
    {
        bool firstSync;
        Action<bool, HttpStatusCode> CallbackOnSuccess;

        public SyncDatabase()
        {
            firstSync = false;
        }

        public void IsFirstSync(bool firstSync)
        {
            this.firstSync = firstSync;
            Helpers.LastUpdatedTime = null;
        }

        public async void PerformSync(Action<bool, HttpStatusCode> callback)
        {
            this.CallbackOnSuccess = callback;
            Helpers.NotificationsLastUpdated = DateTime.UtcNow.ToString("u");
            if (!Helpers.CheckNetworkConnection())
            {
                CallbackOnSuccess(false, HttpStatusCode.ServiceUnavailable);
                return;
            }
            if (firstSync)
            {
                DeleteAllDataInDB();
            }
            //Fetch current user details everytime to sync possible changes made on the website
            CurrentUserRequest request = new CurrentUserRequest();
            await request.GetCurrentUser(_CurrentUserDetailsReceived, _OnErrorReceived);

        }

        private async void _CurrentUserDetailsReceived(User currentUser)
        {
            using (SQLiteConnection dbConn = new SQLiteConnection(Constants.DB_PATH))
            {
                //Insert user details to database
                dbConn.InsertOrReplace(currentUser);

                //Insert picture into database
                currentUser.picture.user_id = currentUser.id;
                dbConn.InsertOrReplace(currentUser.picture);
            }

            //Save current user id in isolated storage
            Helpers.SetCurrentUserId(currentUser.id);

            //Fire next request, i.e. get list of friends
            GetFriendsRequest request = new GetFriendsRequest();
            await request.GetAllFriends(_FriendsDetailsRecevied, _OnErrorReceived);
        }


        private async void _FriendsDetailsRecevied(List<User> friendsList)
        {
            using (SQLiteConnection dbConn = new SQLiteConnection(Constants.DB_PATH))
            {
                List<Balance_User> userBalanceList = new List<Balance_User>();

                dbConn.BeginTransaction();

                foreach (var friend in friendsList)
                {
                    dbConn.InsertOrReplace(friend);

                    Picture picture = friend.picture;
                    picture.user_id = friend.id;
                    dbConn.InsertOrReplace(picture);

                    object[] param = { friend.id };
                    dbConn.Query<Balance_User>("Delete FROM balance_user WHERE user_id= ?", param);

                    foreach (var balance in friend.balance)
                    {
                        balance.user_id = friend.id;
                        dbConn.InsertOrReplace(balance);
                    }
                }
                dbConn.Commit();
            }
            //fetch expenses
            GetExpensesRequest request = new GetExpensesRequest();
            await request.GetAllExpenses(_ExpensesDetailsReceived, _OnErrorReceived);
        }


        private async void _ExpensesDetailsReceived(List<Expense> expensesList)
        {
            if (expensesList != null && expensesList.Count != 0)
            {
                using (SQLiteConnection dbConn = new SQLiteConnection(Constants.DB_PATH))
                {
                    dbConn.BeginTransaction();
                    //Insert expenses
                    foreach (var expense in expensesList)
                    {
                        //The api returns the entire user details of the created by, updated by and deleted by users.
                        //But we only need to store their id's into the database
                        if (expense.created_by != null)
                            expense.created_by_user_id = expense.created_by.id;

                        if (expense.updated_by != null)
                            expense.updated_by_user_id = expense.updated_by.id;

                        if (expense.deleted_by != null)
                            expense.deleted_by_user_id = expense.deleted_by.id;

                        dbConn.InsertOrReplace(expense);

                        if (expense.receipt.original != null || expense.receipt.large != null)
                        {
                            expense.receipt.expense_id = expense.id;
                            dbConn.InsertOrReplace(expense.receipt);
                        }
                    }

                    //Insert debt of each expense (repayments)
                    //Insert expense share users
                    foreach (var expense in expensesList)
                    {
                        //delete users and repayments for this specific expense id as they might have been edited since the last update
                        object[] param = { expense.id };
                        dbConn.Query<Debt_Expense>("Delete FROM debt_expense WHERE expense_id= ?", param);
                        dbConn.Query<Expense_Share>("Delete FROM expense_share WHERE expense_id= ?", param);

                        foreach (var repayment in expense.repayments)
                        {
                            repayment.expense_id = expense.id;
                            dbConn.InsertOrReplace(repayment);
                        }

                        foreach (var expenseUser in expense.users)
                        {
                            expenseUser.expense_id = expense.id;
                            expenseUser.user_id = expenseUser.user.id;
                            dbConn.InsertOrReplace(expenseUser);
                        }
                    }
                    dbConn.Commit();
                }
            }
            //Fetch groups
            GetGroupsRequest request = new GetGroupsRequest();
            await request.GetAllGroups(_GroupsDetailsReceived, _OnErrorReceived);
        }


        private async void _GroupsDetailsReceived(List<Group> groupsList)
        {
            using (SQLiteConnection dbConn = new SQLiteConnection(Constants.DB_PATH))
            {
                dbConn.BeginTransaction();
                //handle the case where some groups might have been deleted.
                dbConn.DeleteAll<Group>();

                //Insert group members
                //Insert debt_group
                foreach (var group in groupsList)
                {
                    dbConn.InsertOrReplace(group);
                    //only care about simplified debts as they are also returned if simplified debts are off

                    //Also don't need the details (group_members and debt_group) for expenses which are not in any group, i.e group_id = 0;
                    if (group.id == 0)
                        continue;
                    else
                    {
                        //delete simplified debts and group member as they might have changed since the last update
                        object[] param = { group.id };
                        dbConn.Query<Group_Members>("Delete FROM group_members WHERE group_id= ?", param);
                        dbConn.Query<Debt_Group>("Delete FROM debt_group WHERE group_id= ?", param);

                        foreach (var debt in group.simplified_debts)
                        {
                            debt.group_id = group.id;
                            dbConn.InsertOrReplace(debt);
                        }


                        foreach (var member in group.members)
                        {
                            Group_Members group_member = new Group_Members()
                            {
                                group_id = group.id,
                                user_id = member.id
                            };
                            dbConn.InsertOrReplace(group_member);
                        }
                    }
                }
                dbConn.Commit();
            }
            GetCurrenciesRequest request = new GetCurrenciesRequest();
            await request.GetSupportedCurrencies(_CurrenciesReceived);
        }

        private void _CurrenciesReceived(List<Currency> currencyList)
        {
            if (currencyList != null && currencyList.Count != 0)
            {
                using (SQLiteConnection dbConn = new SQLiteConnection(Constants.DB_PATH))
                {
                    dbConn.DeleteAll<Currency>();
                    dbConn.InsertAll(currencyList);
                }
            }
            Helpers.LastUpdatedTime = DateTime.UtcNow.ToString("u");
            CallbackOnSuccess(true, HttpStatusCode.OK);
        }

        private void _OnErrorReceived(HttpStatusCode statusCode)
        {
            switch (statusCode)
            {
                case HttpStatusCode.Unauthorized:
                    CallbackOnSuccess(false, HttpStatusCode.Unauthorized);
                    break;
                default:
                    CallbackOnSuccess(false, statusCode);
                    break;
            }
        }

        public static void DeleteAllDataInDB()
        {
            try
            {
                using (SQLiteConnection dbConn = new SQLiteConnection(Constants.DB_PATH))
                {
                    dbConn.DeleteAll<User>();
                    dbConn.DeleteAll<Expense>();
                    dbConn.DeleteAll<Group>();
                    dbConn.DeleteAll<Picture>();
                    dbConn.DeleteAll<Receipt>();
                    dbConn.DeleteAll<Balance_User>();
                    dbConn.DeleteAll<Debt_Expense>();
                    dbConn.DeleteAll<Debt_Group>();
                    dbConn.DeleteAll<Expense_Share>();
                    dbConn.DeleteAll<Group_Members>();
                    dbConn.DeleteAll<Currency>();
                    dbConn.DeleteAll<Notifications>();
                }
            }
            catch { }
        }
    }
}
