using MongoDB.Driver;
using UnityEngine;
using MongoDB.Driver.Builders;
using MongoDB.Bson;
using System.Collections.Generic;

public class Mongo
{
    private const string MONGO_URI ="mongodb://localhost/";
    private const string DATABASE_NAME = "testdb";

    private MongoClient client;
    private MongoServer server;
    private MongoDatabase db;

    private MongoCollection<Model_Account> accounts;
    private MongoCollection<Model_Follow> follows;


    public void Init() {

        client = new MongoClient(MONGO_URI);
        server = client.GetServer();
        db = server.GetDatabase(DATABASE_NAME);

        accounts = db.GetCollection<Model_Account>("account");
        follows = db.GetCollection<Model_Follow>("follows");


        Debug.Log("Database initialize");

    }

    public void Shutdown() {
        client = null;
        server.Shutdown();
        db = null;
    }

    #region Insert
    public bool InsertFollow(string token, string emailOrUsername) {
        Model_Follow newFollow = new Model_Follow();
        newFollow.Sender = new MongoDBRef("account", FindAccountByToken(token)._id);

        //start by gettin our reference to our follow
        if (!Utility.IsEmail(emailOrUsername))
        {
            string[] data = emailOrUsername.Split('#');
            if (data[1] != null) {
                Model_Account follow = FindAccountByUsernameAndDiscriminator(data[0], data[1]);
                if (follow != null)
                {
                    newFollow.Target = new MongoDBRef("account", follow._id);
                }
                else return false;
            }
        }
        else {
            Model_Account follow = FindAccountByEmail(emailOrUsername);
            if (follow != null)
            {
                newFollow.Target = new MongoDBRef("account", follow._id);
            }
            else return false;
        }

        if (newFollow.Target != newFollow.Sender)
        {
            var query = Query.And(
                               Query<Model_Follow>.EQ(u => u.Sender, newFollow.Sender),
                               Query<Model_Follow>.EQ(u => u.Target, newFollow.Target));

            if (follows.FindOne(query) == null) {
                follows.Insert(newFollow);
                return true;
            }
            
        }

        return false;

    }
    public bool InsertAccount(string username, string password, string email) {

        //Check is the email is valid
        if (!Utility.IsEmail(email)) {
            Debug.Log(email + " is not email");
            return false;
        }

        //Check is the username is valid
        if (!Utility.IsUsername(username))
        {
            Debug.Log(username + " is not username");
            return false;
        }

        //Check if th account already exists
        if (FindAccountByEmail(email) != null)
        {
            Debug.Log(email + " is already used");
            return false;
        }


        Model_Account newAccount = new Model_Account();
        newAccount.Username = username;
        newAccount.ShaPassword = password;
        newAccount.Email = email;
        newAccount.Discriminator = "0000";

        int rollCount = 0;
        while (FindAccountByUsernameAndDiscriminator(newAccount.Username, newAccount.Discriminator) != null) {

            newAccount.Discriminator = UnityEngine.Random.Range(0, 99).ToString("0000");

            rollCount++;
            if (rollCount > 1000) {
                Debug.Log("We rolled to amny time, suggest username change");
                return false;
            }
        }

        accounts.Insert(newAccount);
        return true;
    }
    public Model_Account LoginAccount(string usernameOrEmail, string password, int connectionId, string token) {
        Model_Account myAccount = null;
        IMongoQuery query = null;
        if (Utility.IsEmail(usernameOrEmail))
        {
            query = Query.And(
                                Query<Model_Account>.EQ(u => u.Email, usernameOrEmail),
                                Query<Model_Account>.EQ(u => u.ShaPassword, password));

            myAccount = accounts.FindOne(query);
        }
        else {
            string[] data = usernameOrEmail.Split('#');
            if (data != null) {
                query = Query.And(
                                    Query<Model_Account>.EQ(u => u.Username, data[0]),
                                    Query<Model_Account>.EQ(u => u.Discriminator, data[1]),
                                    Query<Model_Account>.EQ(u => u.ShaPassword, password));

                myAccount = accounts.FindOne(query);
            }
        }

        if (myAccount != null) {
            myAccount.ActiveConnection = connectionId;
            myAccount.Token = token;
            myAccount.Status = 1;
            myAccount.LastLogin = System.DateTime.Now;

            accounts.Update(query, Update<Model_Account>.Replace(myAccount));
        }
        else {

        }

        return myAccount;
    }
    #endregion

    #region Fetch
    public Model_Account FindAccountByObjectId(ObjectId id)
    {
        var query = Query<Model_Account>.EQ(u => u._id, id);
        return accounts.FindOne(query);
    }
    public Model_Account FindAccountByEmail(string email) {
        var query = Query<Model_Account>.EQ(u => u.Email, email);
        return accounts.FindOne(query);
    }
    public Model_Account FindAccountByUsernameAndDiscriminator(string username, string discriminator) {

        var query = Query.And(
                            Query<Model_Account>.EQ(u => u.Username, username),
                            Query<Model_Account>.EQ(u => u.Discriminator, discriminator));
        return accounts.FindOne(query);
    }
    public Model_Account FindAccountByToken(string token) {
        var query = Query<Model_Account>.EQ(u => u.Token, token);
        return accounts.FindOne(query);
    }
    public Model_Account FindAccountByConnectionId(int connectionId) {
        var query = Query<Model_Account>.EQ(u => u.ActiveConnection, connectionId);
        return accounts.FindOne(query);
    }

    public List<Account> FindeAllFollowFrom(string token) {
        var self = new MongoDBRef("account", FindAccountByToken(token)._id);
        var query = Query<Model_Follow>.EQ(f => f.Sender, self);

        List<Account> followRespons = new List<Account>();
        foreach (var f in follows.Find(query))
        {
            followRespons.Add(FindAccountByObjectId(f.Target.Id.AsObjectId).GetAccount());
        }

        return followRespons;
    }
    public List<Account> FindeAllFollowBy(string email) {
        var self = new MongoDBRef("account", FindAccountByEmail(email)._id);
        var query = Query<Model_Follow>.EQ(f => f.Target, self);

        List<Account> followRespons = new List<Account>();
        foreach (var f in follows.Find(query))
        {
            followRespons.Add(FindAccountByObjectId(f.Sender.Id.AsObjectId).GetAccount());
        }

        return followRespons;
    }

    public Model_Follow FindFollowByUsernameAndDiscriminator(string token, string usernameAndDiscriminator) {
        string[] data = usernameAndDiscriminator.Split('#');
        if (data[1] != null)
        {
            var sender = new MongoDBRef("account", FindAccountByToken(token)._id);
            var follow = new MongoDBRef("account", FindAccountByUsernameAndDiscriminator(data[0], data[1])._id);
            var query = Query.And(
                   Query<Model_Follow>.EQ(u => u.Sender, sender),
                   Query<Model_Follow>.EQ(u => u.Target, follow));
            return follows.FindOne(query);
        }

        return null;
    }
    #endregion

    #region Update
    public void UpdateAccountAfterDisconnection(string email)
    {
        var query = Query<Model_Account>.EQ(a => a.Email, email); 
        var account = accounts.FindOne(query);

        account.Token = null;
        account.ActiveConnection = 0;
        account.Status = 0;

        accounts.Update(query, Update<Model_Account>.Replace(account));
    }

    #endregion

    #region Delete
    public void RemoveFollow(string token, string UsernameDiscriminator) {

        ObjectId id = FindFollowByUsernameAndDiscriminator(token, UsernameDiscriminator)._id;
        follows.Remove(Query<Model_Follow>.EQ(u => u._id, id));
    }

    #endregion
}
