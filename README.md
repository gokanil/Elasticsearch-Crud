# Elasticsearch-Crud

Bu Uygulamayı Elasticsearch'yi anlamak ve test etmek için cmd gibi komut bazlı bir yapıyla yazdım. Örneğin help komutu ile komutlar listelenebilir.\
Elasticsearch servisini çalıştırmak için uygulama konumundaki konsol penceresine 'docker-compose up -d --scale elasticsearch=2' yazılabilir.\
Buradaki scale komutu elasticsearch servisini 2 container ile çalıştırır. Böylece Pool bağlantı türlerini test ederken daha rahat anlaşılabilir.

</br>

Uygulama Örnek Resim(altını çizdiklerim komutlardır):
![Alt text](/../main/Images/searchLine.png)

</br>

Uygulama İçin Örnek Komutlar:
<pre>
connect true                                                Elasticsearch servisine InMemoryConnection kullanarak bir bağlantı kurar.
connect false localhost:9201                                http://localhost:9201/ adresindeki Elasticsearch servisi ile bir bağlantı kurar.  
connectpool false static localhost:9200 localhost:9201      Belirtilen servis adresleri ile birlikte Elasticsearch servisine StaticConnectionPool kullanarak bir bağlantı kurar.
get indices                                                 Bağlantı kurulan servisteki bütün indeksleri listeler.
get document test                                           İsmi test olan indeksin içindeki bütün verileri listeler.
get alias                                                   Bütün imza ve indeks isimlerini listeler.
add indices test alias                                      test isminde bir indeks oluşturur ve imza ismini alias olarak ayarlar.
add document test                                           test ismindeki indekse girilen json verisini ekler.
   => {"field":"hello world!"}
search test wildcard field *l*                              test ismindeki indeksin içerisindeki field isimli alandaki satırların içerisinde l harfi bulunan bütün satırları listeler.
search test multimatch hello                                test ismindeki indeksin içerisinde hello metini geçen bütün satırları listeler.
</pre>

help komutu ile listelenen komutlar:
<pre>
CONNECT [(inmemory)TRUE/FALSE] [(URL)]: Elasticsearch servisi ile bir bağlantı kurmayı sağlar.
  -[(inmemory)TRUE/FALSE] => True durumunda Elasticsearch servisine ihtiyaç duymadan sahte bir bağlantı kurar. Bu bağlantı ile uygulama içerisindeki ayarlanan verilerle işlem yapar.
  -[(URL)] => Bağlantı kurulacak elasticsearch servisinin adresidir. Değer girilmez ise varsayılan olarak 'http://localhost:9200' adresini kullanır.
CONNECTPOOL [(inmemory)TRUE/FALSE] [(POOL)] [(URL)] : Elasticsearch servisine pool yöntemi ile bir bağlantı kurmayı sağlar.
  -[(inmemory)TRUE/FALSE] => ↑↑
  - [(POOL)] => Pool bağlantı türünü belirtir. Pool bağlantısının 5 çeşidi vardır ve bunlar '? pool' komutu ile listelenebilir.
        -SingleNodeConnectionPool: Elasticsearch, Pool belirtilmeden bağlantı kurulduğunda varsayılan olarak SingleNodeConnectionPool kullanarak bir bağlantı kurar.
        -CloudConnectionPool: Cloud bağlantısı 'CONNECTPOOL [(inmemory)TRUE/FALSE] [(POOL)] [(CLOUDID)] [(USERNAME)] [(PASSWORD)]' şeklinde kullanılmalı.
             -[(CLOUDID)] => 
             -[(USERNAME)] =>
             -[(PASSWORD)] =>
        -StaticConnectionPool: Çoklu servis adresi ile bağlantı kurar. İlk bağlantıda en az 1 başarılı bağlantı kurmalı. Her 'GET', 'ADD' ve 'SEARCH' işlemi yapıldığında bir sonraki bağlantıya geçer(bağlantı başarısız ise sonrakine geçmeye devam eder).
        -SniffingConnectionPool:
        -StickyConnectionPool:
GET [(TERM)] [(INDEX)]: Elasticsearch servisi ile başarılı bir bağlantı kurulduktan sonra getirme işlemi yapar.
  -[(TERM)] => Kendi belirlediğim bazı terim türleridir ve bunlar '? term' komutu ile listelenebilir. Şu an için 4 tür yazdım.
        -indices: İndeksleri belirtir.
        -alias:  İndekslerin imzasını belirtir.
        -document: İndekslerin içindeki verileri belirtir.
        -nodes: Servisin kullandığı Node'yi belirtir. Elasticsearch servisi en az 1 node ile çalışır.
  -[(INDEX)] => İndeksin ismi.
ADD [(TERM)] [(INDEX)] [(ALIAS)]: Elasticsearch servisi ile başarılı bir bağlantı kurulduktan sonra ekleme işlemi yapar.
  -[(TERM)]: ↑↑
  -[(INDEX)]: ↑↑
  -[(ALIAS)]: İmzanın ismi.
SEARCH [(INDEX)] [(QUERY)] [(FIELD)] [(QUERYSTRING)]: Elasticsearch servisi ile başarılı bir bağlantı kurulduktan sonra arama işlemi yapar.
  -[(INDEX)]: ↑↑
  -[(QUERY)]: Aramanın türünü belirtir ve bunlar '? query' komutu ile listelenebilir. Şu an için 10 çeşit arama türünü ekledim.
        -MULTIMATCH: Alan adı belirtmeden bütün alanlarda tam bir kelime araması yapar.
        -MATCH: Alan adına göre tam bir kelime araması yapar.
        -TERM:
        -WILDCARD: Belirtilen özel karakterlere göre arama yapar.
        -PREFIX: Kelimenin başından arama yapar.
        -FUZZY:
        -RANGE:
        -QUERYSTRING:
        -TEXT:
        -MISSING:
   -[(FIELD)]: İndex içerisindeki alan ismidir.
   -[(QUERYSTRING)]: Arama yapılacak metin.
  ? [(COM)]: Komutlar ve türler için yardım ve bilgileri listeler.(yapım aşamasında)
    -[(COM)] => komut türleri.
</pre>

</br>

-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

Bence bu elasticsearch'yi özetleyen en iyi resim. Ayrıca kaynaklarda verdiğim pdfleri incelemenizi tavsiye ederim.\
![Alt text](https://miro.medium.com/max/360/1*gDZfzAqLEnJJdd7dNGqMRA.jpeg)

</br>

Elasticsearch terimleri(alıntı: https://mehmetayhan.com.tr/yazi/elasticsearch-nedir)
<pre>
Index: Her bir veritabanı index olarak belirtilir.
Örnek: eticaret
Type: Veritabanındaki tablolardır.
Örnek: urunler, kategoriler, siparişler…
Field: Tablolar içerisindeki sütunlardır.
Örnek: urunler => id kategori_id adi fiyat
Documents:Tablo içerisindeki kayıtlarımızdır.
Mapping: Veritabanımızdaki schemalardır.
Örnek: string, integer, double, boolean
Shard: Bir index, çok fazla veri barındırdığı zaman node'un donanımsal depolama limitlerini zorlayabilecek duruma gelebilir. Bu sorunu çözmek için elasticsearch, bir index'i birden çok parçaya bölmemize olanak tanıyan "shards" yapısını kullanmakta. Bir index oluşturduğunuzda çok kolay bir şekilde kaç tane shard kullanmak istediğinizi tanımlayabilirsiniz. Her bir shard, bir index'in sahip olduğu tüm özelliklere sahip olan ve cluster'da ki herhangi bir node'da barındırılabilen tamamen bağımsız bir indexe sahiptir.
</pre>

</br>

Kaynaklar:\
https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/nest-getting-started.html \
https://www.slideshare.net/rahuldausa/introduction-to-apache-lucene-solr \
https://www.slideshare.net/SeaseLtd/lets-build-an-inverted-index-introduction-to-apache-lucenesolr \
https://www.borakasmer.com/net-core-uzerinde-elasticsearch-ile-istenen-bir-mesafe-icindeki-lokasyonlarin-filitrelenmesi/ \
https://stackoverflow.com/questions/14977848/how-to-make-sure-that-string-is-valid-json-using-json-net


