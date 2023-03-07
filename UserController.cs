using NHibernate;
using NHibernate.Cfg;
using NHibernate.Criterion;
using PhotonServerDemo.Model;
using System;

namespace PhotonServerDemo
{
    /// <summary>
    /// Create data operation manger class 
    /// </summary>
    class UserController
    {


        private void Parse()
        {


        }

        private ISession GetSession()
        {
            Configuration config = new Configuration();
            config.Configure();//Parse configuration file(hibernate.cfg.xml)
            config.AddAssembly("PhotonServerDemo");
            //build a session and start a transaction
            return config.BuildSessionFactory().OpenSession();

        }
        //Insert data
        public bool Add(User user)
        {
            //auto release
            using (ISession session = GetSession())
            {
                try {

                    using (ITransaction transaction = session.BeginTransaction())
                    {
                        session.Save(user);
                        transaction.Commit();
                        return true;
                    }
                } catch
                {
                    return false;
                }

              
            }
        }

        public bool DeleteByUserName(string userName)
        {
            //auto release
            using (ISession session = GetSession())
            {
                try {
                    using (ITransaction transaction = session.BeginTransaction())
                    {
                        session.Delete(GetData(userName));
                        transaction.Commit();
                        return true;
                    }
                }
                catch {
                    return false;
                }

               
            }
        }
        public void Delete(User user)
        {
            //auto release
            using (ISession session = GetSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    session.Delete(user);
                    transaction.Commit();
                }
            }
        }

        public void Update(User user)
        {
            //auto release
            using (ISession session = GetSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    session.Update(user);
                    transaction.Commit();
                }
            }
        }

        public User GetDataById(int id)
        {
            //auto release
            using (ISession session = GetSession())
            {
                return session.Get<User>(id);
            }
        }
        public User GetData(string name)
        {
            //auto release
            using (ISession session = GetSession())
            {
                return session.CreateCriteria(typeof(User))
                    .Add(Restrictions
                    .Eq("Name", name))
                    .UniqueResult<User>();

            }
        }
        public User GetData(string name, int age)
        {
            //auto release
            using (ISession session = GetSession())
            {
                return session.CreateCriteria(typeof(User))
                    .Add(Restrictions
                    .Eq("Name", name))
                    .Add(Restrictions.Eq("Age", age))
                    .UniqueResult<User>();

            }
        }

        public bool Verify(string name, int age)
        {
            //auto release
            using (ISession session = GetSession())
            {

                User user = session.CreateCriteria(typeof(User))
             .Add(Restrictions
             .Eq("Name", name))
             .Add(Restrictions.Eq("Age", age))
             .UniqueResult<User>();
                if (user != null)
                {
                    Console.WriteLine("Verify Successfully");
                    Console.WriteLine(user.ToString());
                    return true;
                }
                else
                {

                    return false;
                }
            }
        }


    }
}
