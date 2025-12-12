-- MySQL dump 10.13  Distrib 8.0.38, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: artikolly
-- ------------------------------------------------------
-- Server version	8.0.30

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `category`
--

DROP TABLE IF EXISTS `category`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `category` (
  `CategoryID` int NOT NULL AUTO_INCREMENT,
  `CategoryName` varchar(100) NOT NULL,
  PRIMARY KEY (`CategoryID`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `category`
--

LOCK TABLES `category` WRITE;
/*!40000 ALTER TABLE `category` DISABLE KEYS */;
INSERT INTO `category` VALUES (1,'Мужские'),(2,'Женские'),(3,'Унисекс'),(4,'люкс');
/*!40000 ALTER TABLE `category` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `manufacturer`
--

DROP TABLE IF EXISTS `manufacturer`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `manufacturer` (
  `ManufacturerID` int NOT NULL AUTO_INCREMENT,
  `ManufacturerName` varchar(100) NOT NULL,
  `CountryName` varchar(100) NOT NULL,
  PRIMARY KEY (`ManufacturerID`)
) ENGINE=InnoDB AUTO_INCREMENT=52 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `manufacturer`
--

LOCK TABLES `manufacturer` WRITE;
/*!40000 ALTER TABLE `manufacturer` DISABLE KEYS */;
INSERT INTO `manufacturer` VALUES (1,'Chanel','Франция'),(2,'Dior','Франция'),(3,'Hermès','Франция'),(4,'Maison Francis Kurkdjian','Франция'),(5,'Tom Ford','США'),(6,'Byredo','Франция'),(7,'Givenchy','Франция'),(8,'Zielinski & Rozen','Польша'),(9,'Montale','Франция'),(10,'Faberlic','Россия'),(11,'Lacoste','Франция'),(12,'Dolce & Gabbana','Италия'),(13,'FM by Federico Mahora','Германия'),(14,'Jo Malone','Великобритания'),(15,'Gucci','Италия'),(16,'Yodeyma','Испания'),(17,'Yves Rocher','Франция'),(18,'Giorgio Armani','Италия'),(19,'S Parfum','Россия'),(20,'Guerlain','Франция'),(21,'Clive Christian','Великобритания'),(22,'Sergio Tacchini','Италия'),(23,'Escentric Molecules','Германия'),(24,'Mancera','Франция'),(25,'Dzintars','Латвия'),(26,'Antonio Banderas','Испания'),(27,'Brioni','Италия'),(28,'Amouage','Оман'),(29,'Omerta','Италия'),(30,'Kenzo','Франция'),(31,'Новая Заря','Россия'),(32,'Versace','Италия'),(33,'Brocard','Россия'),(34,'Bershka','Испания'),(35,'Kilian Paris','Франция'),(36,'Burberry','Великобритания'),(37,'Lancôme','Франция'),(38,'Hugo Boss','Германия'),(39,'Bvlgari','Италия'),(40,'Северное Сияние','Россия'),(41,'Amway','США'),(42,'Yves Saint Laurent','Франция'),(43,'Vince Camuto','США'),(44,'Trussardi','Италия'),(45,'Lalique','Франция'),(46,'Pull & Bear','Испания'),(47,'Moschino','Италия'),(48,'Tiziana Terenzi','Италия'),(49,'Victoria\'s Secret','США'),(50,'Chloé','Франция'),(51,'Armani','Франция');
/*!40000 ALTER TABLE `manufacturer` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `order`
--

DROP TABLE IF EXISTS `order`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `order` (
  `OrderID` int NOT NULL AUTO_INCREMENT,
  `OrderDate` date NOT NULL,
  `OrderUser` int NOT NULL,
  `OrderStatus` int NOT NULL,
  `OrderTotalPrice` decimal(19,2) NOT NULL,
  `OrderTotalDiscount` decimal(19,2) NOT NULL,
  PRIMARY KEY (`OrderID`),
  KEY `order_ibfk_2_idx` (`OrderStatus`),
  KEY `order_ibfk_1_idx` (`OrderUser`),
  CONSTRAINT `order_ibfk_1` FOREIGN KEY (`OrderUser`) REFERENCES `User` (`UserID`),
  CONSTRAINT `order_ibfk_2` FOREIGN KEY (`OrderStatus`) REFERENCES `Status` (`StatusID`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `order`
--

LOCK TABLES `order` WRITE;
/*!40000 ALTER TABLE `order` DISABLE KEYS */;
INSERT INTO `order` VALUES (1,'2024-01-05',3,1,50000.00,15.00),(2,'2024-01-06',5,2,125678.00,20.00),(3,'2024-01-07',7,3,48566.00,19.00),(4,'2024-01-08',9,1,86963.00,22.00),(5,'2024-01-09',11,4,45565.00,10.00),(6,'2024-01-10',14,2,42885.00,23.00),(7,'2024-01-11',16,3,9655.00,10.00),(8,'2024-01-12',18,1,4555.00,5.00),(9,'2024-01-13',20,4,535353.00,13.00),(10,'2024-01-14',22,2,45375.00,4.00),(11,'2024-01-15',24,3,2823.00,3.00),(12,'2024-01-16',26,1,52345.00,19.00);
/*!40000 ALTER TABLE `order` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `orderproduct`
--

DROP TABLE IF EXISTS `orderproduct`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `orderproduct` (
  `OrderID` int NOT NULL,
  `ProductArticleNumber` varchar(100) NOT NULL,
  `Count` int NOT NULL,
  PRIMARY KEY (`OrderID`,`ProductArticleNumber`),
  KEY `ProductArticleNumber` (`ProductArticleNumber`),
  CONSTRAINT `orderproduct_ibfk_1` FOREIGN KEY (`OrderID`) REFERENCES `order` (`OrderID`),
  CONSTRAINT `orderproduct_ibfk_2` FOREIGN KEY (`ProductArticleNumber`) REFERENCES `Product` (`ProductArticleNumber`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `orderproduct`
--

LOCK TABLES `orderproduct` WRITE;
/*!40000 ALTER TABLE `orderproduct` DISABLE KEYS */;
INSERT INTO `orderproduct` VALUES (1,'A00001',2),(1,'A00002',1),(1,'A00003',1),(1,'A00004',2),(1,'A00021',1),(2,'A00005',2),(2,'A00006',2),(2,'A00007',2),(2,'A00008',3),(2,'A00022',1),(3,'A00009',3),(3,'A00010',3),(3,'A00023',2),(4,'A00011',1),(4,'A00012',2),(4,'A00024',1),(4,'A00044',2),(5,'A00013',1),(5,'A00014',2),(5,'A00015',1),(5,'A00025',2),(5,'A00035',2),(5,'A00045',1),(6,'A00016',3),(6,'A00026',1),(6,'A00036',2),(6,'A00046',3),(7,'A00017',2),(7,'A00027',1),(7,'A00037',2),(7,'A00047',1),(8,'A00018',1),(8,'A00028',1),(8,'A00038',1),(8,'A00048',1),(9,'A00019',3),(9,'A00020',2),(9,'A00029',1),(9,'A00039',1),(9,'A00049',2),(10,'A00030',1),(10,'A00031',2),(10,'A00040',3),(10,'A00043',1),(10,'A00050',2),(11,'A00032',1),(11,'A00041',2),(12,'A00033',3),(12,'A00034',3),(12,'A00042',2);
/*!40000 ALTER TABLE `orderproduct` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `product`
--

DROP TABLE IF EXISTS `product`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `product` (
  `ProductArticleNumber` varchar(6) NOT NULL,
  `ProductName` varchar(100) NOT NULL,
  `ProductCost` float NOT NULL,
  `ProductDiscount` tinyint NOT NULL,
  `ProductManufacture` int NOT NULL,
  `ProductCategory` int NOT NULL,
  `ProductQuantilyStock` int NOT NULL,
  `ProductDescription` varchar(100) NOT NULL,
  `Volume` varchar(6) NOT NULL,
  `ProductPhoto` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`ProductArticleNumber`),
  KEY `Quantity` (`ProductCategory`),
  KEY `ProductManufacture_idx` (`ProductManufacture`),
  CONSTRAINT `ProductManufacture` FOREIGN KEY (`ProductManufacture`) REFERENCES `manufacturer` (`ManufacturerID`),
  CONSTRAINT `Quantity` FOREIGN KEY (`ProductCategory`) REFERENCES `category` (`CategoryID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `product`
--

LOCK TABLES `product` WRITE;
/*!40000 ALTER TABLE `product` DISABLE KEYS */;
INSERT INTO `product` VALUES ('34','ghfg',4556,45,19,1,10,'rthrthrth','100ml',NULL),('A00001','Chanel No.5 Eau de Parfum',9000,10,1,2,50,'Люксовый женский аромат от Chanel','50ml','chanel_no5.jpg'),('A00002','Dior Sauvage Eau de Toilette',6000,15,2,1,40,'Мужской аромат от Dior, нишевый','100ml','dior_sauvage.jpg'),('A00003','Hermès Terre d\'Hermès',7125,5,3,1,30,'Люксовый мужской аромат от Hermès','100ml','hermes_terre.jpg'),('A00004','Avon Little Black Dress',1875,20,4,2,100,'Доступный женский аромат, массовый','30ml','avon_lbd.jpg'),('A00005','Tom Ford Black Orchid',11250,10,5,2,20,'Люксовый нишевый женский аромат от Tom Ford','50ml','tomford_blackorchid.jpg'),('A00006','Oriflame Eclat Femme',2250,10,6,2,70,'Массовый женский аромат','50ml','oriflame_eclat.jpg'),('A00007','Givenchy Ange ou Démon',8250,12,7,2,25,'Люксовый женский аромат','50ml','givenchy_ange.jpg'),('A00008','Zielinski & Rozen Amber',3375,8,8,3,60,'Унисекс нишевый аромат','30ml','zielinski_amber.jpg'),('A00009','Montale Roses Musk',9750,5,9,3,15,'Нишевый унисекс','50ml','montale_roses.jpg'),('A00010','Faberlic Exotic',1500,25,10,3,200,'Массовый унисекс аромат','50ml','faberlic_exotic.jpg'),('A00011','Lacoste Blanc',5250,10,11,1,80,'Мужской массовый аромат','100ml','lacoste_blanc.jpg'),('A00012','Dolce & Gabbana Light Blue',6375,10,12,2,90,'Женский популярный аромат','100ml','dg_lightblue.jpg'),('A00013','FM Federico Mahora 471',2625,15,13,3,50,'Унисекс нишевый аромат','50ml','faberico471.jpg'),('A00014','Jo Malone Peony & Blush Suede',9375,10,14,2,40,'Люксовый женский аромат','30ml','jomalone_peony.jpg'),('A00015','Gucci Guilty Pour Homme',7125,10,15,1,35,'Люксовый мужской аромат','90ml','gucci_guilty.jpg'),('A00016','Yodeyma Vanilla',3000,12,16,3,100,'Массовый унисекс с ванилью','50ml','yodeyma_vanilla.jpg'),('A00017','Yves Rocher Petitgrain',2250,20,17,1,60,'Массовый мужской аромат','100ml','yves_rocher_petitgrain.jpg'),('A00018','Giorgio Armani Si',8625,10,18,2,45,'Люксовый женский аромат','50ml','armani_si.jpg'),('A00019','S Parfum Musk',1875,15,19,3,80,'Массовый унисекс парфюм','50ml','sparfum_musk.jpg'),('A00020','Guerlain Shalimar',10500,8,20,2,20,'Люксовый женский аромат','50ml','guerlain_shalimar.jpg'),('A00021','Clive Christian X',18750,5,21,1,10,'Люксовый мужской парфюм','50ml','clive_x.jpg'),('A00022','Sergio Tacchini Sport',3375,10,22,1,55,'Массовый мужской спорт аромат','100ml','sergio_sport.jpg'),('A00023','Escentric Molecules Molecule 01',9000,10,23,3,25,'Нишевый унисекс','100ml','escentric_molecule.jpg'),('A00024','Mancera Cedrat Boise',9750,7,24,3,15,'Нишевый унисекс','120ml','mancera_cedrat.jpg'),('A00025','Dzintars Amber',1875,20,25,2,80,'Массовый женский аромат','30ml','dzintars_amber.jpg'),('A00026','Antonio Banderas Blue Seduction',2625,15,26,1,70,'Массовый мужской аромат','100ml','antonio_banderas_blue.jpg'),('A00027','Brioni Uomo',11250,10,27,1,20,'Люксовый мужской парфюм','100ml','brioni_uomo.jpg'),('A00028','Amouage Reflection',13500,12,28,2,12,'Люксовый женский аромат','100ml','amouage_reflection.jpg'),('A00029','Omerta Noir',5250,10,29,1,40,'Массовый мужской аромат','100ml','omerta_noir.jpg'),('A00030','Kenzo Jungle',6375,8,30,2,35,'Женский нишевый аромат','100ml','kenzo_jungle.jpg'),('A00031','Новая Заря Classic',2250,20,31,2,150,'Массовый женский аромат','50ml','nova_zarya_classic.jpg'),('A00032','Versace Eros',7125,10,32,1,45,'Люксовый мужской аромат','100ml','versace_eros.jpg'),('A00033','Brocard Classic',1500,25,33,3,200,'Унисекс нишевый','50ml','brocard_classic.jpg'),('A00034','Bershka Fresh',1125,30,34,3,300,'Массовый унисекс','30ml','bershka_fresh.jpg'),('A00035','Kilian Black Phantom',21000,5,35,2,10,'Люксовый нишевый','50ml','kilian_blackphantom.jpg'),('A00036','Burberry Brit',6000,10,36,2,60,'Массовый женский аромат','100ml','burberry_brit.jpg'),('A00037','Lancôme La Vie Est Belle',6750,10,37,2,80,'Люксовый женский аромат','50ml','lancome_belle.jpg'),('A00038','Hugo Boss Bottled',5625,10,38,1,65,'Массовый мужской аромат','100ml','hugo_boss_bottled.jpg'),('A00039','Bvlgari Man in Black',9000,8,39,1,25,'Люксовый мужской','100ml','bvlgari_man.jpg'),('A00040','Северное Сияние Arctic',2625,15,40,3,90,'Унисекс нишевый','50ml','sever_sila.jpg'),('A00041','Amway Artistry',3750,10,41,2,55,'Массовый женский','50ml','amway_artistry.jpg'),('A00042','Yves Saint Laurent Black Opium',8250,10,42,2,40,'Люксовый женский','50ml','ysl_blackopium.jpg'),('A00043','Vince Camuto Fierce',6375,10,43,1,50,'Массовый мужской','100ml','vince_fierce.jpg'),('A00044','Trussardi My Name',5250,10,44,2,45,'Массовый женский','50ml','trussardi_myname.jpg'),('A00045','Lalique Encre Noire',7125,8,45,1,30,'Люксовый мужской','100ml','lalique_encrenoire.jpg'),('A00046','Pull & Bear Fresh',900,20,46,3,150,'Массовый унисекс','30ml','pull_bear_fresh.jpg'),('A00047','Moschino Toy Boy',7500,10,47,1,25,'Массовый мужской','50ml','moschino_toyboy.jpg'),('A00048','Tiziana Terenzi Orion',22500,5,48,3,8,'Нишевый унисекс','100ml','tiziana_orion.jpg'),('A00049','Victoria\'s Secret Bombshell',3375,15,49,2,70,'Массовый женский','50ml','victoria_bombshell.jpg'),('A00050','Chloé Eau de Parfum',9375,10,50,2,35,'Люксовый женский','50ml','chloe_eau.jpg'),('A56785','dgnfg dfgh fh',56768,56,29,3,35,'sdgdsfgdsg','100ml',NULL);
/*!40000 ALTER TABLE `product` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `role`
--

DROP TABLE IF EXISTS `role`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `role` (
  `RoleId` int NOT NULL AUTO_INCREMENT,
  `RoleName` varchar(45) NOT NULL,
  PRIMARY KEY (`RoleId`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `role`
--

LOCK TABLES `role` WRITE;
/*!40000 ALTER TABLE `role` DISABLE KEYS */;
INSERT INTO `role` VALUES (1,'Администратор'),(2,'Товаровед'),(3,'Продавец'),(4,'Клиент'),(5,'администра');
/*!40000 ALTER TABLE `role` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Status`
--

DROP TABLE IF EXISTS `Status`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Status` (
  `StatusID` int NOT NULL AUTO_INCREMENT,
  `StatusName` varchar(100) NOT NULL,
  PRIMARY KEY (`StatusID`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Status`
--

LOCK TABLES `Status` WRITE;
/*!40000 ALTER TABLE `Status` DISABLE KEYS */;
INSERT INTO `Status` VALUES (1,'В ожидании'),(2,'Отгружен'),(3,'Доставлен'),(4,'В обработке');
/*!40000 ALTER TABLE `Status` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `suppliers`
--

DROP TABLE IF EXISTS `suppliers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `suppliers` (
  `SuppliersID` int NOT NULL AUTO_INCREMENT,
  `SuppliersName` varchar(100) NOT NULL,
  `ContactInformation` varchar(150) NOT NULL,
  PRIMARY KEY (`SuppliersID`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `suppliers`
--

LOCK TABLES `suppliers` WRITE;
/*!40000 ALTER TABLE `suppliers` DISABLE KEYS */;
INSERT INTO `suppliers` VALUES (1,'Компания А','Тел: +7 900 123-45-67, email: contact@companya.ru'),(2,'Компания Б','Тел: +7 901 234-56-78, email: info@companyb.ru'),(3,'Поставщик C','Тел: +7 902 345-67-89, email: sales@providerc.ru'),(4,'Оптовик D','Тел: +7 903 456-78-90, email: support@wholesaled.ru'),(5,'Дистрибьютор E','Тел: +7 904 567-89-01, email: distrib@edist.ru'),(6,'Компания F','Тел: +7 905 678-90-12, email: contact@companyf.ru'),(7,'Поставщик G','Тел: +7 906 789-01-23, email: sales@providerg.ru'),(8,'Партнер H','Тел: +7 907 890-12-34, email: partner@hpartner.ru'),(9,'Экспортер I','Тел: +7 908 901-23-45, email: export@iexporter.ru'),(10,'Дистрибьютор J','Тел: +7 909 012-34-56, email: distrib@jdistrib.ru'),(11,'Производитель K','Тел: +7 910 123-45-67, email: info@kproducer.ru'),(12,'Компания L','Тел: +7 911 234-56-78, email: contact@companyl.ru'),(13,'Поставщик M','Тел: +7 912 345-67-89, email: sales@providerm.ru'),(14,'Оптовик N','Тел: +7 913 456-78-90, email: support@nwholesale.ru'),(15,'Компания O','Тел: +7 914 567-89-01, email: contact@companyo.ru');
/*!40000 ALTER TABLE `suppliers` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `supplies`
--

DROP TABLE IF EXISTS `supplies`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `supplies` (
  `SuppliesID` int NOT NULL AUTO_INCREMENT,
  `DateOfDelivery` date NOT NULL,
  `SupplierID` int NOT NULL,
  `ProductArticle` varchar(45) NOT NULL,
  `Quantity` int NOT NULL,
  PRIMARY KEY (`SuppliesID`),
  KEY `SupplierID_idx` (`SupplierID`),
  KEY `ProductArticle_idx` (`ProductArticle`),
  CONSTRAINT `ProductArticle` FOREIGN KEY (`ProductArticle`) REFERENCES `product` (`ProductArticleNumber`),
  CONSTRAINT `SupplierID` FOREIGN KEY (`SupplierID`) REFERENCES `suppliers` (`SuppliersID`)
) ENGINE=InnoDB AUTO_INCREMENT=51 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `supplies`
--

LOCK TABLES `supplies` WRITE;
/*!40000 ALTER TABLE `supplies` DISABLE KEYS */;
INSERT INTO `supplies` VALUES (1,'2024-01-10',1,'A00001',20),(2,'2024-01-12',2,'A00002',15),(3,'2024-01-15',3,'A00003',10),(4,'2024-01-20',4,'A00004',50),(5,'2024-01-22',5,'A00005',5),(6,'2024-01-25',6,'A00006',70),(7,'2024-01-28',7,'A00007',25),(8,'2024-02-01',8,'A00008',40),(9,'2024-02-05',9,'A00009',12),(10,'2024-02-10',10,'A00010',100),(11,'2024-02-12',11,'A00011',30),(12,'2024-02-15',12,'A00012',45),(13,'2024-02-18',13,'A00013',20),(14,'2024-02-20',14,'A00014',35),(15,'2024-02-22',15,'A00015',8),(16,'2024-02-25',1,'A00016',60),(17,'2024-02-28',2,'A00017',90),(18,'2024-03-01',3,'A00018',15),(19,'2024-03-05',4,'A00019',25),(20,'2024-03-10',5,'A00020',10),(21,'2024-03-12',6,'A00021',5),(22,'2024-03-15',7,'A00022',55),(23,'2024-03-18',8,'A00023',28),(24,'2024-03-20',9,'A00024',12),(25,'2024-03-22',10,'A00025',80),(26,'2024-03-25',11,'A00026',40),(27,'2024-03-28',12,'A00027',18),(28,'2024-04-01',13,'A00028',10),(29,'2024-04-05',14,'A00029',70),(30,'2024-04-08',15,'A00030',22),(31,'2024-04-10',1,'A00031',100),(32,'2024-04-12',2,'A00032',45),(33,'2024-04-15',3,'A00033',60),(34,'2024-04-18',4,'A00034',130),(35,'2024-04-20',5,'A00035',9),(36,'2024-04-22',6,'A00036',40),(37,'2024-04-25',7,'A00037',75),(38,'2024-04-28',8,'A00038',25),(39,'2024-05-01',9,'A00039',14),(40,'2024-05-03',10,'A00040',55),(41,'2024-05-05',11,'A00041',35),(42,'2024-05-08',12,'A00042',20),(43,'2024-05-10',13,'A00043',13),(44,'2024-05-12',14,'A00044',27),(45,'2024-05-15',15,'A00045',9),(46,'2024-05-18',1,'A00046',150),(47,'2024-05-20',2,'A00047',20),(48,'2024-05-22',3,'A00048',8),(49,'2024-05-25',4,'A00049',70),(50,'2024-05-28',5,'A00050',25);
/*!40000 ALTER TABLE `supplies` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user`
--

DROP TABLE IF EXISTS `user`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user` (
  `UserID` int NOT NULL AUTO_INCREMENT,
  `UserSurname` varchar(100) NOT NULL,
  `UserName` varchar(100) NOT NULL,
  `UserPatronomic` varchar(100) DEFAULT NULL,
  `DateOfBirth` date NOT NULL,
  `Phone` varchar(20) NOT NULL,
  `Login` tinytext NOT NULL,
  `Password` tinytext NOT NULL,
  `Role` int NOT NULL,
  PRIMARY KEY (`UserID`),
  KEY `Role` (`Role`),
  CONSTRAINT `Role` FOREIGN KEY (`Role`) REFERENCES `role` (`RoleId`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=54 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user`
--

LOCK TABLES `user` WRITE;
/*!40000 ALTER TABLE `user` DISABLE KEYS */;
INSERT INTO `user` VALUES (1,'Иванов','Иван','Иванович','1980-01-15','+7(900)111-11-1','ivanov1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',1),(2,'Петров','Петр','Петрович','1975-05-20','+7(900)222-22-2','petrov1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',2),(3,'Смирнова','Анна','Андреевна','1990-03-10','+7(900)333-33-3','smirnova1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(4,'Кузнецов','Алексей','Владимирович','1985-07-22','+7(900)444-44-4','kuznetsov1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',2),(5,'Лебедева','Мария','Павловна','1992-12-05','+7(900)555-55-5','lebedova1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(6,'Новиков','Дмитрий','Михайлович','1988-09-17','+7(900)666-66-6','novikov1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',1),(7,'Морозова','Ольга','Сергеевна','1983-04-25','+7(900)777-77-7','morozova1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(8,'Соколов','Андрей','Васильевич','1978-11-11','+7(900)888-88-8','sokolov1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',2),(9,'Борисова','Ирина','Петровна','1986-02-14','+7(900)999-99-9','borisova1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(10,'Егоров','Максим','Александрович','1991-08-30','+7(901)111-11-1','egorov1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',2),(11,'Федорова','Елена','Викторовна','1983-11-30','+7(901)222-22-2','fedorova2','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(12,'Павлов','Роман','Игоревич','1979-06-12','+7(901)333-33-3','pavlov1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',1),(13,'Ковалев','Юрий','Михайлович','1982-09-09','+7(901)444-44-4','kovalev1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',2),(14,'Тарасова','Наталья','Андреевна','1987-03-03','+7(901)555-55-5','tarasova1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(15,'Михайлов','Владимир','Григорьевич','1984-10-10','+7(901)666-66-6','mikhaylov1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',1),(16,'Баранова','Оксана','Павловна','1993-07-07','+7(901)777-77-7','baranova1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(17,'Фролов','Денис','Александрович','1989-05-05','+7(901)888-88-8','frolov1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',2),(18,'Алексеева','Татьяна','Владимировна','1981-12-24','+7(901)999-99-9','alexejeva1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(19,'Григорьев','Константин','Иванович','1977-04-22','+7(902)111-11-1','grigoryev1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',2),(20,'Лазарева','Виктория','Юрьевна','1994-09-15','+7(902)222-22-2','lazareva1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(21,'Романов','Андрей','Петрович','1983-02-02','+7(902)333-33-3','romanov1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',1),(22,'Крылова','Светлана','Михайловна','1986-01-25','+7(902)444-44-4','krylova1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(23,'Васильев','Игорь','Андреевич','1979-11-11','+7(902)555-55-5','vasilev1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',2),(24,'Степанова','Нина','Ивановна','1990-06-18','+7(902)666-66-6','stepanova1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(25,'Титов','Владимир','Петрович','1982-08-08','+7(902)777-77-7','titiv1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',1),(26,'Мельникова','Анастасия','Григорьевна','1991-10-10','+7(902)888-88-8','melnikova1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(27,'Капустин','Павел','Владимирович','1976-12-12','+7(902)999-99-9','kapustin1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',2),(28,'Карпова','Ольга','Андреевна','1984-03-03','+7(903)111-11-1','karpova1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(29,'Никитин','Алексей','Иванович','1988-07-07','+7(903)222-22-2','nikitin1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',2),(30,'Захарова','Людмила','Павловна','1993-04-04','+7(903)333-33-3','zaharova1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(31,'Яковлева','Екатерина','Викторовна','1985-05-05','+7(903)444-44-4','yakovleva1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(32,'Белов','Денис','Владимирович','1987-09-09','+7(903)555-55-5','belov1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',2),(33,'Гаврилова','Марина','Игоревна','1989-11-11','+7(903)666-66-6','gavrila1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(34,'Воронова','Тамара','Петровна','1982-02-02','+7(903)777-77-7','voronova1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(35,'Рыбаков','Михаил','Александрович','1978-03-03','+7(903)888-88-8','rybakov1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',2),(36,'Соловьева','Алена','Михайловна','1994-06-06','+7(903)999-99-9','soloveva1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(37,'Панова','Ирина','Викторовна','1983-07-07','+7(904)111-11-1','panova1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(38,'Дмитриева','Наталья','Петровна','1986-08-08','+7(904)222-22-2','dmitrieva1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(39,'Филатов','Андрей','Иванович','1977-09-09','+7(904)333-33-3','filatov1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',2),(40,'Климова','Светлана','Владимировна','1990-10-10','+7(904)444-44-4','klimova1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(41,'Маслов','Владимир','Петрович','1981-11-11','+7(904)555-55-5','maslov1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',1),(42,'Горбунова','Оксана','Андреевна','1984-12-12','+7(904)666-66-6','gorbunova1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(43,'Чернов','Павел','Михайлович','1979-01-01','+7(904)777-77-7','chernov1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',2),(44,'Щербакова','Елена','Викторовна','1987-02-02','+7(904)888-88-8','shcherbakova1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(45,'Жуков','Алексей','Петрович','1982-03-03','+7(904)999-99-9','zhukov1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',2),(46,'Балашова','Татьяна','Ивановна','1993-04-04','+7(905)111-11-1','balashova1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(47,'Кравцов','Владимир','Васильевич','1980-05-05','+7(905)222-22-2','kravtsov1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',1),(48,'Лазарева','Виктория','Юрьевна','1985-06-06','+7(905)333-33-3','lazareva2','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(49,'Морозов','Денис','Петрович','1987-07-07','+7(905)444-44-4','morozov1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',2),(50,'Федорова','Елена','Викторовна','1983-11-30','+7(905)555-55-5','fedorova3','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',3),(51,'Томин','Андрей','Дмитриевич','2006-06-13','+7(996) 829-16-31','547547rht','1',1),(52,'Ф','Ф','Ф','2000-12-12','+7(054) 440-45-53','564','645645',1),(53,'Ф','Ф','Ф','2000-12-12','+7(054) 440-45-53','564','61f943138cefd2616fbe93b5d24f68147362db9cfc9bbb52e5a1f2a6fd5f967b',1);
/*!40000 ALTER TABLE `user` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping events for database 'artikolly'
--

--
-- Dumping routines for database 'artikolly'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-10-29 18:07:11
