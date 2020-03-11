using EfDemo.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfDemo.Persistence.Test
{
    [TestClass]
    public class ApplicationDbContextTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            // Build the ApplicationDbContext 
            //  - with InMemory-DB
            return new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .EnableSensitiveDataLogging()
                .Options);
        }

        [TestMethod]
        public async Task ApplicationDbContext_AddSchoolClass_ShouldPersistSchoolClass()
        {
            string dbName = Guid.NewGuid().ToString();

            using (ApplicationDbContext dbContext = GetDbContext(dbName))
            {
                SchoolClass schoolClass = new SchoolClass { Name = "6ABIF_6AKIF" };
                Assert.AreEqual(0, schoolClass.Id);
                dbContext.SchoolClasses.Add(schoolClass);
                await dbContext.SaveChangesAsync();
                Assert.AreNotEqual(0, schoolClass.Id);
            }

            using (ApplicationDbContext verifyContext = GetDbContext(dbName))
            {
                StringBuilder logText = new StringBuilder();
                Assert.AreEqual(1, await verifyContext.SchoolClasses.CountAsync());
                SchoolClass schoolClass = await verifyContext.SchoolClasses.FirstAsync();
                Assert.IsNotNull(schoolClass);
                Assert.AreEqual("6ABIF_6AKIF", schoolClass.Name);

            }
        }

        [TestMethod]
        public async Task ApplicationDbContext_AddSchoolClassWithPupils_QueryPupils_ShouldReturnPupils()
        {
            string dbName = Guid.NewGuid().ToString();

            using (ApplicationDbContext dbContext = GetDbContext(dbName))
            {
                SchoolClass schoolClass = new SchoolClass
                {
                    Name = "6ABIF_6AKIF",
                    Pupils = new List<Pupil>
                    {
                        new Pupil
                        {
                            FirstName = "Max",
                            LastName = "Mustermann",
                            BirthDate = DateTime.Parse("1.1.1990")
                        },
                        new Pupil
                        {
                            FirstName = "Eva",
                            LastName = "Musterfrau",
                            BirthDate = DateTime.Parse("1.1.1991")
                        },
                        new Pupil
                        {
                            FirstName = "Fritz",
                            LastName = "Musterkind",
                            BirthDate = DateTime.Parse("1.1.1980")
                        },
                        new Pupil
                        {
                            FirstName = "Franz", LastName = "Huber", BirthDate = DateTime.Parse("10.7.1999")
                        }
                    }
                };
                dbContext.SchoolClasses.Add(schoolClass);
                await dbContext.SaveChangesAsync();
                Assert.AreNotEqual(0, schoolClass.Id);
            }

            using (ApplicationDbContext queryContext = GetDbContext(dbName))
            {
                Assert.AreEqual(1, await queryContext.SchoolClasses.CountAsync());
                Assert.AreEqual(4, await queryContext.Pupils.CountAsync());
                // Ältester Schüler
                Pupil eldest = await queryContext.Pupils.OrderBy(pupil => pupil.BirthDate).FirstAsync();
                Assert.AreEqual("Musterkind", eldest.LastName);
            }
        }


        [TestMethod]
        public async Task ApplicationDbContext_UpdateSchoolClass_ShouldReturnChangedSchoolClass()
        {
            string dbName = Guid.NewGuid().ToString();

            using (ApplicationDbContext dbContext = GetDbContext(dbName))
            {
                SchoolClass schoolClass = new SchoolClass { Name = "5ABIF_5AKIF" };
                Assert.AreEqual(0, schoolClass.Id);
                dbContext.SchoolClasses.Add(schoolClass);
                await dbContext.SaveChangesAsync();
                Assert.AreNotEqual(0, schoolClass.Id);
            }

            using (ApplicationDbContext updateContext = GetDbContext(dbName))
            {
                SchoolClass schoolClass = await updateContext.SchoolClasses.FirstAsync();
                schoolClass.Name = "6ABIF_6AKIF";
                await updateContext.SaveChangesAsync();
            }

            using (ApplicationDbContext verifyContext = GetDbContext(dbName))
            {
                SchoolClass schoolClass = await verifyContext.SchoolClasses.FirstAsync();
                Assert.AreEqual("6ABIF_6AKIF", schoolClass.Name);
            }
        }

        [TestMethod]
        public async Task ApplicationDbContext_DeleteSchoolClass_ShouldReturnZeroSchoolClasses()
        {
            string dbName = Guid.NewGuid().ToString();

            using (ApplicationDbContext dbContext = GetDbContext(dbName))
            {
                SchoolClass schoolClass = new SchoolClass { Name = "6ABIF_6AKIF" };
                Assert.AreEqual(0, schoolClass.Id);
                dbContext.SchoolClasses.Add(schoolClass);
                dbContext.SchoolClasses.Add(new SchoolClass { Name = "5ABIF_5AKIF" });
                await dbContext.SaveChangesAsync();
                Assert.AreNotEqual(0, schoolClass.Id);
            }

            using (ApplicationDbContext deleteContext = GetDbContext(dbName))
            {
                SchoolClass schoolClass = await deleteContext.SchoolClasses.SingleAsync(sc => sc.Name == "5ABIF_5AKIF");
                deleteContext.SchoolClasses.Remove(schoolClass);
                deleteContext.SaveChanges();
            }

            using (ApplicationDbContext verifyContext = GetDbContext(dbName))
            {
                Assert.AreEqual(1, await verifyContext.SchoolClasses.CountAsync());
                var schoolClass = await verifyContext.SchoolClasses.FirstAsync();
                Assert.AreEqual("6ABIF_6AKIF", schoolClass.Name);
            }
        }

        //HomeWork Tests
        [TestMethod]
        public async Task ApplicationDbConext_SortPupilsThenDelete()
        {
            string dbName = Guid.NewGuid().ToString();

            using (ApplicationDbContext dbContext = GetDbContext(dbName))
            {
                SchoolClass schoolClass = new SchoolClass
                {
                    Name = "SomeClassName_1",
                    Pupils = new List<Pupil>
                    {
                        new Pupil
                        {
                            FirstName = "Student",
                            LastName = "First",
                            BirthDate = DateTime.Parse("1.3.2003")
                        },
                        new Pupil
                        {
                            FirstName = "Student",
                            LastName = "Second",
                            BirthDate = DateTime.Parse("8.4.2003")
                        },
                        new Pupil
                        {
                            FirstName = "Student",
                            LastName = "Third",
                            BirthDate = DateTime.Parse("7.8.2003")
                        },
                        new Pupil
                        {
                            FirstName = "Student",
                            LastName = "Fourth",
                            BirthDate = DateTime.Parse("5.12.2002")
                        },
                        new Pupil
                        {
                            FirstName = "Student",
                            LastName = "Fith",
                            BirthDate = DateTime.Parse("28.10.2002")
                        }
                    }
                };

                dbContext.SchoolClasses.Add(schoolClass);
                await dbContext.SaveChangesAsync();
                Assert.AreNotEqual(0, schoolClass.Id);
            }

            using (ApplicationDbContext sortAndDeleteContext = GetDbContext(dbName))
            {
                var pupils = sortAndDeleteContext.Pupils.OrderBy(p => p.LastName);

                Pupil firstPupil = await pupils.FirstAsync();
                sortAndDeleteContext.Remove(firstPupil);
                await sortAndDeleteContext.SaveChangesAsync();

                Pupil secondPupil = await pupils.FirstAsync();
                sortAndDeleteContext.Remove(secondPupil);
                await sortAndDeleteContext.SaveChangesAsync();

                Pupil thirdPupil = await pupils.FirstAsync();
                sortAndDeleteContext.Remove(thirdPupil);
                await sortAndDeleteContext.SaveChangesAsync();

                Pupil fourthPupil = await pupils.FirstAsync();
                sortAndDeleteContext.Remove(fourthPupil);
                await sortAndDeleteContext.SaveChangesAsync();

                Pupil fithPupil = await pupils.FirstAsync();
                sortAndDeleteContext.Remove(fithPupil);
                await sortAndDeleteContext.SaveChangesAsync();

                Assert.AreEqual("First", firstPupil.LastName);
                Assert.AreEqual("Fith", secondPupil.LastName);
                Assert.AreEqual("Fourth", thirdPupil.LastName);
                Assert.AreEqual("Second", fourthPupil.LastName);
                Assert.AreEqual("Third", fithPupil.LastName);
            }
        }

        [TestMethod]
        public async Task ApplicationDbContext_DeleteOnePupil_ShouldReturnFour()
        {
            string dbName = Guid.NewGuid().ToString();

            using (ApplicationDbContext dbContext = GetDbContext(dbName))
            {
                SchoolClass schoolClass = new SchoolClass
                {
                    Name = "SomeClassName_1",
                    Pupils = new List<Pupil>
                    {
                        new Pupil
                        {
                            FirstName = "Student",
                            LastName = "First",
                            BirthDate = DateTime.Parse("1.3.2003")
                        },
                        new Pupil
                        {
                            FirstName = "Student",
                            LastName = "Second",
                            BirthDate = DateTime.Parse("8.4.2003")
                        },
                        new Pupil
                        {
                            FirstName = "Student",
                            LastName = "Third",
                            BirthDate = DateTime.Parse("7.8.2003")
                        },
                        new Pupil
                        {
                            FirstName = "Student",
                            LastName = "Fourth",
                            BirthDate = DateTime.Parse("5.12.2002")
                        },
                        new Pupil
                        {
                            FirstName = "Student",
                            LastName = "Fith",
                            BirthDate = DateTime.Parse("28.10.2002")
                        }
                    }
                };
                dbContext.SchoolClasses.Add(schoolClass);
                await dbContext.SaveChangesAsync();
                Assert.AreNotEqual(0, schoolClass.Id);
            }

            using (ApplicationDbContext deletePupil = GetDbContext(dbName))
            {
                Pupil pupil = await deletePupil.Pupils.SingleAsync(p => p.LastName == "Third");
                deletePupil.Pupils.Remove(pupil);
                deletePupil.SaveChanges();
            }

            using (ApplicationDbContext queryContext = GetDbContext(dbName))
            {
                Assert.AreEqual(4, await queryContext.Pupils.CountAsync());
            }
        }
    }
}
