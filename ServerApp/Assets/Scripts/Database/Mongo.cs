using MongoDB.Driver;
using UnityEngine;
using MongoDB.Driver.Builders;

public class Mongo
{
    private const string MONGO_URI ="mongodb://localhost/";
    private const string DATABASE_NAME = "testdb";

    private MongoClient client;
    private MongoServer server;
    private MongoDatabase db;

    private MongoCollection<Model_Account> accounts;

    public void Init() {

        client = new MongoClient(MONGO_URI);
        server = client.GetServer();
        db = server.GetDatabase(DATABASE_NAME);

        accounts = db.GetCollection<Model_Account>("account");

        Debug.Log("Database initialize");

    }

    public void Shutdown() {
        client = null;
        server.Shutdown();
        db = null;
    }

    #region Insert
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
        while (FindaccountByUsernameAndDiscriminator(newAccount.Username, newAccount.Discriminator) != null) {

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
    public Model_Account FindAccountByEmail(string email) {
        var query = Query<Model_Account>.EQ(u => u.Email, email);
        return accounts.FindOne(query);
    }

    public Model_Account FindaccountByUsernameAndDiscriminator(string username, string discriminator) {

        var query = Query.And(
                            Query<Model_Account>.EQ(u => u.Username, username),
                            Query<Model_Account>.EQ(u => u.Discriminator, discriminator));
        return accounts.FindOne(query);
    }
    #endregion

    #region Update
    #endregion

    #region Delete
    #endregion
}
