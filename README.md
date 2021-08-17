# Elasticsearch-Crud
Bu Uygulamayı Elasticsearch'yi anlamak ve test etmek için cmd gibi komut bazlı bir yapıyla yazdım. Örneğin help komutu ile komutlar listelenebilir.\
Elasticsearch servisini çalıştırmak için uygulama konumundaki konsol penceresine 'docker-compose up -d --scale elasticsearch=2' yazılabilir.\
Buradaki scale komutu elasticsearch servisini 2 container ile çalıştırır*. Böylece Pool bağlantı türlerini test ederken daha rahat anlaşılabilir.

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
        -PREFIX: Kelimenin başı*
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




