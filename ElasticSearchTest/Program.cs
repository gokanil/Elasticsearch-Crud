using Elasticsearch.Net;
using ElasticSearchTest.Helper;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ElasticSearchTest
{
    internal class Program
    {
        /*
         * ---örnek komutlar---
         * connect true
         * connect false localhost:9201
         * connectpool false static localhost:9200 localhost:9201 localhost:9202
         * get indices
         * get document test
         * get alias
         * add indices test alias
         * add document test
         *    => {"field":"hello world!"}
         * search test wildcard field *l*
         * search test multimatch hello
         */

        static List<string> consoleHistory = new List<string>();
        static ElasticClient elasticClient;
        static Uri defaultUrl = new Uri("http://localhost:9200");

        static object exampleResponse = new
        {
            took = 1,
            timed_out = false,
            _shards = new
            {
                total = 2,
                successful = 2,
                failed = 0
            },
            hits = new
            {
                total = new { value = 25 },
                max_score = 1.0,
                hits = Enumerable.Range(1, 25).Select(i => (Object)new
                {
                    _index = "project",
                    _type = "project",
                    _id = $"Project {i}",
                    _score = 1.0,
                    _source = new { name = $"Project {i}" }
                }).ToArray()
            }
        };

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += ExceptionHelper.UnhandledExceptionHandler;

            WriteLine("# Elasticsearch ile ilgili işlemler yapabilmeniz için öncelikle servis ile bir bağlantı kurmanız gerekir.");
            WriteLine("# Komut listesi için 'HELP' komutunu kullanabilirsiniz.");
            WriteLine();

            while (true)
            {
                string commandParts = ReadLine();
                processCommand(commandParts.Split(' '));
            }
        }

        static void processCommand(string[] commandParts)
        {
            string command = commandParts.ElementAtOrDefault(0)?.ToLowerInvariant();
            string inMemory = commandParts.ElementAtOrDefault(1)?.ToLowerInvariant();
            string term = commandParts.ElementAtOrDefault(1)?.ToLowerInvariant();
            string subCommand = commandParts.ElementAtOrDefault(1)?.ToLowerInvariant();
            string searchIndex = commandParts.ElementAtOrDefault(1);
            string url = commandParts.ElementAtOrDefault(2);
            string indexName = commandParts.ElementAtOrDefault(2);
            string queryType = commandParts.ElementAtOrDefault(2)?.ToLowerInvariant();
            string pool = commandParts.ElementAtOrDefault(2)?.ToLowerInvariant();
            string alias = commandParts.ElementAtOrDefault(3);
            string type = commandParts.ElementAtOrDefault(3)?.ToLowerInvariant();
            string cloudId = commandParts.ElementAtOrDefault(3);
            string poolUrl = commandParts.ElementAtOrDefault(3);
            List<string> poolUrlList = commandParts.Skip(3).ToList();
            string multiMatchQueryString = string.Join(' ', commandParts.Skip(3));
            string queryString = string.Join(' ', commandParts.Skip(4));
            string userName = commandParts.ElementAtOrDefault(4);
            string password = commandParts.ElementAtOrDefault(5);

            if (command == "h" || command == "help")
            {
                writeHelpCommands();
                return;
            }
            else if (command == "?")
            {
                if (subCommand == "p" || subCommand == "pool" || subCommand == "pools")
                    WritePoolCalls();
                else if (subCommand == "t" || subCommand == "term" || subCommand == "terms")
                    WriteTermCalls();
                else if (subCommand == "q" || subCommand == "query" || subCommand == "queries")
                    WriteQueryCalls();
                return;
            }

            if (elasticClient == null && ((command != "c" && command != "connect") && (command != "cp" && command != "connectpool")))
                WriteLine("Lüfen öncelikle ElasticSearch servisi ile bir bağlantı kurun. Nasıl bağlanacağınızı görebilmek için 'HELP' komutunu kullanabilirsiniz.");
            else if (command == "c" || command == "connect")
            {
                bool.TryParse(inMemory, out bool inMemoryResult);
                Uri urlResult = new WebProxy(url)?.Address;

                ElasticSearchConnect(inMemory: inMemoryResult, uri: urlResult);
            }
            else if (command == "cp" || command == "connectpool")
            {
                bool.TryParse(inMemory, out bool result);

                if (pool == null)
                    ElasticSearchConnect(inMemory: result);
                if (pool == "1" || pool == "s" || pool == "sn" || pool == "singlenode" || pool == "singlenodeconnectionpool")
                    SingleNodeConnectionPool(poolUrl, inMemory: result);
                if (pool == "2" || pool == "c" || pool == "cc" || pool == "cloud" || pool == "cloudconnectionpool")
                    CloudConnectionPool(cloudId, userName, password, inMemory: result);
                if (pool == "3" || pool == "st" || pool == "sc" || pool == "static" || pool == "staticconnectionpool")
                    StaticConnectionPool(poolUrlList, inMemory: result);
                if (pool == "4" || pool == "sc" || pool == "scp" || pool == "sniffing" || pool == "sniffingconnectionpool")
                    SniffingConnectionPool(poolUrlList, inMemory: result);
                if (pool == "5" || pool == "stc" || pool == "cky" || pool == "sticky" || pool == "stickyconnectionpool")
                    StickyConnectionPool(poolUrlList, inMemory: result);
            }
            else if (command == "g" || command == "get")
            {
                Get(term, indexName);
            }
            else if (command == "a" || command == "add")
            {
                Add(term, indexName, alias);
            }
            else if (command == "s" || command == "search")
            {
                QueryContainer query = null;

                if (queryType == "1" || queryType == "mm" || queryType == "multimatch")
                    query = GetMultiMatchQuery(multiMatchQueryString);
                else if (queryType == "2" || queryType == "m" || queryType == "match")
                    query = GetMatchQuery(type, queryString);
                else if (queryType == "4" || queryType == "w" || queryType == "wildcard")
                    query = GetWildcardQuery(type, queryString);

                Search(searchIndex, query);
            }
        }


        #region Console
        static void writeHelpCommands()
        {
            WriteLine("# URL varsayılan olarak 'http://localhost:9200' olarak ayarlanmıştır.");
            WriteLine("# Elasticsearch servisi ile başarılı bir bağlantı kurulduktan sonra 'GET', 'ADD' ve 'SEARCH' işlemleri yapılabilir.");
            WriteLine("# POOL, TERM veya QUERY listesi için komut satırına örneğin: '? POOL' yazabilirsiniz.");
            WriteLine();
            WriteLine("CONNECT [(inmemory)TRUE/FALSE] [(URL)]                Elasticsearch servisi ile bir bağlantı kurmayı sağlar.");
            WriteLine("CONNECTPOOL [(inmemory)TRUE/FALSE] [(POOL)] [(URL)]   Elasticsearch servisine pool yöntemi ile bir bağlantı kurar.");
            WriteLine("GET [(TERM)] [(INDEX)]                                Getirme işlemi yapar.");
            WriteLine("ADD [(TERM)] [(INDEX)] [(ALIAS)]                      Ekleme işlemi yapar.");
            WriteLine("SEARCH [(INDEX)] [(QUERY)] [(FIELD)] [(QUERYSTRING)]  Arama işlemi yapar.");
            WriteLine("? [(COM)]                                             Komutlar ve türler için yardım ve bilgileri listeler.");
            WriteLine();
        }

        static void WritePoolCalls()
        {
            WriteLine("# CLOUD bağlantısını şu şekilde kullanmalısınız: 'CONNECTPOOL [(inmemory)TRUE/FALSE] [(POOL)] [(CLOUDID)] [(USERNAME)] [(PASSWORD)]'");
            WriteLine();
            WriteLine("------CONNECTION POOLS-----");
            WriteLine("1:SINGLENODE");
            WriteLine("2:CLOUD");
            WriteLine("3:STATIC");
            WriteLine("4:SNIFFING");
            WriteLine("5:STICKY");
            WriteLine();
        }

        static void WriteTermCalls()
        {
            WriteLine("------TERMS-----");
            WriteLine("1:INDICES");
            WriteLine("2:ALIAS");
            WriteLine("3:DOCUMENT");
            WriteLine("4:NODES");
            WriteLine();
        }

        static void WriteQueryCalls()
        {
            WriteLine("------QUERIES-----");
            WriteLine("1:MULTIMATCH");
            WriteLine("2:MATCH");
            WriteLine("3:TERM");
            WriteLine("4:WILDCARD");
            WriteLine("5:PREFIX");
            WriteLine("6:FUZZY");
            WriteLine("7:RANGE");
            WriteLine("8:QUERYSTRING");
            WriteLine("9:TEXT");
            WriteLine("10:MISSING");
            WriteLine();
        }

        //soruce: https://stackoverflow.com/questions/14977848/how-to-make-sure-that-string-is-valid-json-using-json-net
        static bool TryParse(string strInput, ref object jToken)
        {
            if (String.IsNullOrWhiteSpace(strInput))
                return false;

            strInput = strInput.Trim();

            if (!(strInput.StartsWith("{") && strInput.EndsWith("}")) && //For object
                !(strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
                return false;

            try
            {
                jToken = JsonConvert.DeserializeObject<object>(strInput);
                return true;
            }
            catch (JsonReaderException jex)
            {
                return false;
            }
            catch
            {
                return false;
            }
        }

        static void WriteLine(string value = null)
        {
            consoleHistory.Add(value);
            Console.WriteLine(value);
        }

        static void Write(string value = null)
        {
            consoleHistory.Add(value);
            Console.Write(value);
        }

        static string ReadLine()
        {
            string value = "=> ";
            Console.Write(value);
            string enter = Console.ReadLine();
            consoleHistory.Add(value + enter);
            return enter;
        }
        #endregion


        #region Crud
        static void Get(string term, string indexName)
        {
            if (term == "indices")
            {
                var result = elasticClient.Cat.Indices(x => x.AllIndices());
                //var result = elasticClient.Indices.Get(new GetIndexRequest(Indices.All));

                if (result.OriginalException == null)
                    WriteLine($"Getirme İşlemi Başarısız! Hata: '{result.OriginalException.Message}'");

                foreach (var indices in result.Records.ToList())
                    WriteLine($"{indices.Index} | size: {indices.StoreSize} | docs: {indices.DocsCount}");
            }
            else if (term == "document")
            {
                var response = elasticClient.Search<dynamic>(s => s.Index(indexName));

                if (response.OriginalException != null)
                    WriteLine($"Getirme İşlemi Başarısız! Hata: '{response.OriginalException.Message}'");

                foreach (var document in response.Documents)
                    WriteLine(JsonConvert.SerializeObject(document));
            }
            else if (term == "alias")
            {
                var result = elasticClient.Cat.Aliases();
                //var result = elasticClient.GetAliasesPointingToIndex("altest");

                if (result.OriginalException != null)
                    WriteLine($"Getirme İşlemi Başarısız! Hata: '{result.OriginalException.Message}'");

                foreach (var alias in result.Records.Where(x => x.Index == (indexName ?? x.Index)).ToList())
                    WriteLine($"alias: {alias.Alias} | index: {alias.Index}");
            }
            else if (term == "nodes")
            {
                var result = elasticClient.Cat.Nodes();

                if (result.OriginalException != null)
                    WriteLine($"Getirme İşlemi Başarısız! Hata: '{result.OriginalException.Message}'");

                foreach (var node in result.Records.ToList())
                    WriteLine($"Name: {node.Name} | Ip: {node.Ip} | CPU: {node.CPU}");
            }
        }

        static void Add(string term, string indexName, string alias)
        {
            if (term == "indices")
            {
                var response = elasticClient.Indices.Create(indexName,
                 x => x.Map<object>(m => m.AutoMap()).Aliases(a => alias == null ? null : a.Alias(alias)));

                if (response.OriginalException == null)
                    WriteLine("Ekeleme başarılı!");
                else
                    WriteLine($"Ekleme İşlemi Başarısız! Hata: '{response.OriginalException.Message}'");
            }
            else if (term == "document")
            {
                if (string.IsNullOrEmpty(indexName))
                {
                    WriteLine("UYARI: İndeks ismi boş olamaz.");
                    return;
                }

                Console.Clear();
                Console.WriteLine("Json Data:");
                Console.Write("=> ");
                string jsonLine = "";
                object jToken = null;
                string ent = "";
                do
                {
                    ent = Console.ReadLine();
                    jsonLine += ent;
                    if (TryParse(jsonLine, ref jToken))
                        break;

                } while (!string.IsNullOrEmpty(ent));
                Console.Clear();
                foreach (var history in consoleHistory)
                    Console.WriteLine(history);

                if (jToken == null)
                    return;

                try
                {
                    var response = elasticClient.LowLevel.Index<IndexResponse>(indexName, PostData.String(jToken.ToString()), null);

                    if (response.OriginalException == null)
                        WriteLine("Ekeleme başarılı!");
                    else
                        WriteLine($"Ekleme İşlemi Başarısız! Hata: '{response.OriginalException.Message}'");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"HATA: {ex.Message}");
                }
            }
            else if (term == "alias")
            {
                var response = elasticClient.Indices.Create(indexName,
                 x => x.Map<object>(m => m.AutoMap()).Aliases(a => alias == null ? null : a.Alias(alias)));

                if (response.OriginalException == null)
                    WriteLine("Ekeleme başarılı!");
                else
                    WriteLine($"Ekleme İşlemi Başarısız! Hata: '{response.OriginalException.Message}'");
            }
            else if (term == "alias")
            {
                WriteLine("Bu şekilde node ekleyemessin!");
            }
        }

        static void Search(string indexName, QueryContainer query)
        {
            //var termQuery = new TermQuery
            //{
            //    Field = "field",
            //    Value = "value"
            //};

            var searchRequest = new SearchRequest<dynamic>(indexName) { Query = query };
            var searchResponse = elasticClient.Search<dynamic>(searchRequest);

            if (searchResponse.OriginalException == null)
                WriteLine($"Getirme İşlemi Başarısız! Hata: '{searchResponse.OriginalException.Message}'");

            foreach (var document in searchResponse.Documents)
                WriteLine(JsonConvert.SerializeObject(document));
        }

        static QueryContainer GetMatchQuery(string field, string eq)
        {
            return new QueryContainerDescriptor<dynamic>().Match(t => t.Field(field).Query(eq));
        }

        static QueryContainer GetWildcardQuery(string field, string eq)
        {
            return new QueryContainerDescriptor<dynamic>().Wildcard(t => t.Field(field).Value(eq));
        }

        static QueryContainer GetMultiMatchQuery(string eq)
        {
            return new QueryContainerDescriptor<dynamic>().MultiMatch(t => t.Query(eq));
        }
        #endregion


        #region Connections
        static void ElasticSearchConnect(IConnectionPool pool = null, bool inMemory = false, Uri uri = null)
        {


            var responseBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(exampleResponse));

            if (uri != null)
            {
                if (inMemory)
                    WriteLine("Uyarı: InMemory false olarak değiştirildi. Bir url ile birlikte InMemory kullanmak istiyorsanız pool kullanarak bağlanmalısınız.");
                elasticClient = new ElasticClient(new ConnectionSettings(uri).DisableDirectStreaming());
            }
            else if (pool != null)
                elasticClient = new ElasticClient(new ConnectionSettings(pool, inMemory ? new InMemoryConnection(responseBytes) : null).DisableDirectStreaming());
            else
                elasticClient = new ElasticClient(new ConnectionSettings(inMemory ? new InMemoryConnection(responseBytes) : null).DisableDirectStreaming().DefaultDisableIdInference().DisableMetaHeader());


            //
            var Health = elasticClient.Cluster.Health(new ClusterHealthRequest());// { WaitForStatus = WaitForStatus.Red }

            WriteLine();
            ((IConnectionPool)elasticClient.ConnectionSettings.ConnectionPool)?.Nodes.ToList().ForEach(x => { WriteLine($"Servis Adresi: {x.Uri.AbsoluteUri}"); });
            WriteLine();

            string statusMessage = "Bağlantı Kuruldu!";
            if (Health.OriginalException != null)
            {
                statusMessage = $"Bağlantı Başarısız! Error: '{Health.OriginalException.Message}'";
                elasticClient = null;
            }

            Write($"{statusMessage} InMemory: {inMemory}");
            if (pool != null)
                Write($" Pool: {pool.ToString()},");
            WriteLine($" Durum: {Health.Status}");
            //
        }

        static void SingleNodeConnectionPool(string url = null, bool inMemory = false)
        {
            var result = new WebProxy(url)?.Address;
            var pool = new SingleNodeConnectionPool(result ?? defaultUrl);
            ElasticSearchConnect(pool, inMemory);
        }

        static void CloudConnectionPool(string cloudId, string userName, string password, bool inMemory = false)
        {
            var credentials = new BasicAuthenticationCredentials(userName, password);
            var pool = new CloudConnectionPool(cloudId, credentials);
            ElasticSearchConnect(pool, inMemory);
        }

        static void StaticConnectionPool(List<string> urlList = null, bool inMemory = false)
        {
            var uriList = urlList.Select(x => new WebProxy(x)?.Address).ToList();
            var pool = new StaticConnectionPool(uriList.Any() ? uriList : new List<Uri>() { defaultUrl });
            ElasticSearchConnect(pool, inMemory);
        }

        static void SniffingConnectionPool(List<string> urlList = null, bool inMemory = false)
        {
            var uriList = urlList.Select(x => new WebProxy(x)?.Address).ToList();
            var pool = new SniffingConnectionPool(uriList.Any() ? uriList : new List<Uri>() { defaultUrl });
            ElasticSearchConnect(pool, inMemory);
        }

        static void StickyConnectionPool(List<string> urlList = null, bool inMemory = false)
        {
            var uriList = urlList.Select(x => new WebProxy(x)?.Address).ToList();
            var pool = new StickyConnectionPool(uriList.Any() ? uriList : new List<Uri>() { defaultUrl });
            ElasticSearchConnect(pool, inMemory);
        }
        #endregion
    }
}