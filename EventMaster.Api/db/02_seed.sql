-- =========================================================
-- 02_seed.sql  (MySQL 8)
-- Assumes DB: eventmasterdb
-- Tables: users, venues, events, event_occurrences, bookings,
--         payments, reviews, replies, media
-- Notes:
--   - events.image stores filename only (event_001.png..)
--   - reviews/replies only for Completed occurrences
--   - media left empty
-- =========================================================

USE eventmasterdb;

START TRANSACTION;

-- -------------------------
-- Users (10)
-- -------------------------
INSERT INTO users (user_id, role, name, age, phone, email, username, password,status)
VALUES
(1,'CUSTOMER','Aanya Patel',22,'5195550141','aanya.patel@mail.com','aanya_p','$2a$11$MRsCGd2YIU1GqlV8/kKl3eRyvPdMi4q6dmMMcc9T1kNkcMl4T6k7C','Active'),
(2,'CUSTOMER','Ethan Martin',25,'4165550198','ethan.martin@mail.com','emartin','$2a$11$MRsCGd2YIU1GqlV8/kKl3eRyvPdMi4q6dmMMcc9T1kNkcMl4T6k7C','Active'),
(3,'CUSTOMER','Noor Ahmed',24,'6475550112','noor.ahmed@mail.com','noorahmed','$2a$11$MRsCGd2YIU1GqlV8/kKl3eRyvPdMi4q6dmMMcc9T1kNkcMl4T6k7C','Active'),
(4,'CUSTOMER','Olivia Chen',23,'6045550184','olivia.chen@mail.com','oliviachen','$2a$11$MRsCGd2YIU1GqlV8/kKl3eRyvPdMi4q6dmMMcc9T1kNkcMl4T6k7C','Active'),
(5,'CUSTOMER','Liam Tremblay',26,'5145550129','liam.tremblay@mail.com','ltremblay','$2a$11$MRsCGd2YIU1GqlV8/kKl3eRyvPdMi4q6dmMMcc9T1kNkcMl4T6k7C','Active'),
(6,'ORGANIZER','MapleLeaf Live Events',30,'4165550100','bookings@mapleleaflive.ca','mapleleaflive','$2a$11$MRsCGd2YIU1GqlV8/kKl3eRyvPdMi4q6dmMMcc9T1kNkcMl4T6k7C','Active'),
(7,'ORGANIZER','Northern Lights Comedy',32,'9055550101','team@northernlightscomedy.ca','nlcomedy','$2a$11$MRsCGd2YIU1GqlV8/kKl3eRyvPdMi4q6dmMMcc9T1kNkcMl4T6k7C','Active'),
(8,'ORGANIZER','WestCoast Concerts',29,'6045550102','info@westcoastconcerts.ca','westcoastconcerts','$2a$11$MRsCGd2YIU1GqlV8/kKl3eRyvPdMi4q6dmMMcc9T1kNkcMl4T6k7C','Active'),
(9,'ORGANIZER','Prairie Sports Group',35,'4035550103','events@prairiesports.ca','prairiesports','$2a$11$MRsCGd2YIU1GqlV8/kKl3eRyvPdMi4q6dmMMcc9T1kNkcMl4T6k7C','Active'),
(10,'ORGANIZER','Théâtre du Centre',33,'5145550104','admin@theatrecentre.ca','theatrecentre','$2a$11$MRsCGd2YIU1GqlV8/kKl3eRyvPdMi4q6dmMMcc9T1kNkcMl4T6k7C','Active');

-- -------------------------
-- Venues (18)
-- -------------------------
INSERT INTO venues (venue_id, name, address, city, province, postal_code, capacity, seating)
VALUES
(1,'Rogers Arena','800 Griffiths Way','Vancouver','BC','V6B 6G1',7500,0),
(2,'Queen Elizabeth Theatre','630 Hamilton St','Vancouver','BC','V6B 5N6',1250,1),
(3,'BC Place','777 Pacific Blvd','Vancouver','BC','V6B 4Y8',10000,0),
(4,'Rogers Centre','1 Blue Jays Way','Toronto','ON','M5V 1J1',10000,0),
(5,'Scotiabank Arena','40 Bay St','Toronto','ON','M5J 2X2',10000,0),
(6,'Massey Hall','178 Victoria St','Toronto','ON','M5B 1T7',3500,0),
(7,'Meridian Hall','1 Front St E','Toronto','ON','M5E 1B2',2500,0),
(8,'Centre Bell','1909 Av des Canadiens-de-Montréal','Montréal','QC','H4B 5G0',8500,0),
(9,'Place des Arts','175 Sainte-Catherine St W','Montréal','QC','H2X 1Z8',750,1),
(10,'Videotron Centre','250 Wilfrid-Hamel Blvd','Québec City','QC','G1L 5A7',1800,1),
(11,'Scotiabank Saddledome','555 Saddledome Rise SE','Calgary','AB','T2G 2W1',6000,0),
(12,'BMO Centre','20 Roundup Way SE','Calgary','AB','T2G 2W1',10000,0),
(13,'Rogers Place','10220 104 Ave NW','Edmonton','AB','T5J 0H6',4000,0),
(14,'National Arts Centre','1 Elgin St','Ottawa','ON','K1P 5W1',200,1),
(15,'Kitchener Memorial Auditorium','400 East Ave','Kitchener','ON','N2H 1Z6',800,1),
(16,'Centre In The Square','101 Queen St N','Kitchener','ON','N2H 6P7',500,1),
(17,'Maxwell’s Concerts & Events','35 University Ave E','Waterloo','ON','N2J 2V9',400,1),
(18,'THEMUSEUM Event Space','10 King St W','Kitchener','ON','N2G 1A3',300,1);

-- -------------------------
-- Events (55)
-- org mapping:
--  comedy -> 7, sports -> 9, concerts -> 8, parties -> 6, theaters -> 10
-- image filename only
-- -------------------------
INSERT INTO events (event_id, org_id, name, category, description, image)
VALUES
-- Comedy (1-11)
(1,7,'Laugh Lines: Stand-Up Night','comedy','A fast-paced stand-up showcase featuring local headliners.','event_001.png'),
(2,7,'The Toronto Roast Room','comedy','A friendly roast night with surprise guest appearances.','event_002.png'),
(3,7,'Montreal Mic Drop','comedy','Bilingual comedy night with rotating performers.','event_003.png'),
(4,7,'Vancouver Punchline Hour','comedy','One-hour feature set with an opening comic.','event_004.png'),
(5,7,'Prairie Laugh Fest','comedy','A regional comedy mini-festival with multiple acts.','event_005.png'),
(6,7,'Late Night Improv Jam','comedy','Improv games, audience prompts, and quick scenes.','event_006.png'),
(7,7,'Clean Comedy Classics','comedy','Family-friendly lineup with clean material only.','event_007.png'),
(8,7,'Campus Comedy Showcase','comedy','Student comics and alumni surprise set.','event_008.png'),
(9,7,'The Friday Giggle Club','comedy','Friday night laughs to kick off the weekend.','event_009.png'),
(10,7,'Comedy & Coffee Live','comedy','Early evening comedy with cafe vibes.','event_010.png'),
(11,7,'Newcomers Night: Fresh Jokes','comedy','New talent night — first-timers welcome.','event_011.png'),

-- Sports (12-21)
(12,9,'Toronto Titans vs Montreal Meteors','sports','Rivalry game night with fan-zone activities.','event_012.png'),
(13,9,'Vancouver Waves vs Calgary Peaks','sports','West showdown featuring halftime contests.','event_013.png'),
(14,9,'Ottawa Capitals vs Edmonton Northstars','sports','National matchup with pre-game warmup access.','event_014.png'),
(15,9,'Battle of the Prairies: Calgary vs Edmonton','sports','Regional derby with special edition merch.','event_015.png'),
(16,9,'Canada Cup: Rising Stars Showcase','sports','Top prospects compete in an exhibition.','event_016.png'),
(17,9,'Winter Classic: Outdoor Showdown','sports','Seasonal special in an outdoor-style production.','event_017.png'),
(18,9,'Playoff Watch Party Live','sports','Big-screen watch party with giveaways.','event_018.png'),
(19,9,'All-Star Skills Night','sports','Skills competitions and player intros.','event_019.png'),
(20,9,'Derby Day: City Rivalry Match','sports','High-intensity rivalry matchup.','event_020.png'),
(21,9,'Championship Night: Finals Game','sports','Finals atmosphere with trophy ceremony.','event_021.png'),

-- Concerts (22-35)
(22,8,'Aurora Beats: Live Tour','concerts','High-energy electronic pop tour night.','event_022.png'),
(23,8,'Northern Lights Symphony Night','concerts','Orchestral program featuring Canadian composers.','event_023.png'),
(24,8,'Indie Shores: Vancouver Sessions','concerts','Indie lineup with coastal soundscapes.','event_024.png'),
(25,8,'Maple Strings: Acoustic Evening','concerts','Acoustic set with storytelling between songs.','event_025.png'),
(26,8,'Electric Skyline: EDM Live','concerts','DJ set with visuals and late-night energy.','event_026.png'),
(27,8,'Jazz After Dark: Montreal','concerts','Jazz quartet + late set jam session.','event_027.png'),
(28,8,'Rock the Harbour: Toronto','concerts','Rock night with opening band showcase.','event_028.png'),
(29,8,'Prairie Pulse: Live Music Night','concerts','Live bands and local spotlight acts.','event_029.png'),
(30,8,'Pop Icons Tribute Night','concerts','Tribute hits night with full band.','event_030.png'),
(31,8,'Lo-Fi Lounge Live Set','concerts','Chill lo-fi set and ambient visuals.','event_031.png'),
(32,8,'Classical Spotlight: Piano & Strings','concerts','Piano trio and string ensemble feature.','event_032.png'),
(33,8,'Sunset Serenade Concert','concerts','Golden-hour themed set list and lighting.','event_033.png'),
(34,8,'Hip-Hop North: Live Showcase','concerts','Hip-hop artists with featured DJ.','event_034.png'),
(35,8,'City Beats Festival','concerts','Multi-act night across genres.','event_035.png'),

-- Parties (36-45)
(36,6,'Neon Saturdays: Downtown Glow Party','parties','Weekly neon-themed Saturday night party.','event_036.png'),
(37,6,'Winter Wonderland Masquerade','parties','Dress-up masquerade with winter theme.','event_037.png'),
(38,6,'Rooftop Friday: Skyline Nights','parties','Friday rooftop-style party atmosphere.','event_038.png'),
(39,6,'Retro 90s Throwback Party','parties','90s music and throwback visuals.','event_039.png'),
(40,6,'Latin Night Social: Salsa & Bachata','parties','Latin dance social with warm-up lesson.','event_040.png'),
(41,6,'Afrobeats & Amapiano Night','parties','Afrobeats + amapiano DJ night.','event_041.png'),
(42,6,'Bollywood Bash: Dance Night','parties','Bollywood dance party with hits set.','event_042.png'),
(43,6,'After Hours: Midnight Lounge','parties','Late-night lounge vibes and DJ set.','event_043.png'),
(44,6,'Valentine’s Weekend: Love & Lights Party','parties','Valentine’s weekend party event.','event_044.png'),
(45,6,'New Year Countdown Party','parties','Countdown party with midnight moment.','event_045.png'),

-- Theaters (46-55)
(46,10,'Hamlet: A Modern Re-Tell','theaters','A modern staging of Hamlet with minimalist design.','event_046.png'),
(47,10,'The Phantom Encore','theaters','Classic-style production with dramatic score.','event_047.png'),
(48,10,'The Maple Leaf Musical','theaters','Original musical celebrating Canadian stories.','event_048.png'),
(49,10,'Midnight on Queen Street (Drama)','theaters','Urban drama set in downtown nightlife.','event_049.png'),
(50,10,'Comedy of Errors: Stage Revival','theaters','Classic comedy with modern pace.','event_050.png'),
(51,10,'The Great Canadian Heist (Play)','theaters','Comedy-thriller heist play.','event_051.png'),
(52,10,'A Winter’s Tale: Live Theatre','theaters','Seasonal theatrical production.','event_052.png'),
(53,10,'The Last Train to Montreal (Drama)','theaters','Character-driven drama on a night train.','event_053.png'),
(54,10,'Lights of Vancouver (Stage Show)','theaters','Stage show celebrating Vancouver lights and sound.','event_054.png'),
(55,10,'The Prairie Dawn (Theatre)','theaters','New play exploring prairie life and resilience.','event_055.png');

-- -------------------------
-- Event Occurrences (132)
-- Mix of Completed (mostly 2025), Scheduled (2026+), Cancelled (mix)
-- Note: seats_occupied only meaningful for seating=1 venues; leave NULL for seating=0
-- -------------------------
INSERT INTO event_occurrences
(occurrence_id, event_id, date, time, price, venue_id, remaining_capacity, seats_occupied, status)
VALUES
(1001,1,'2025-11-15','19:00:00',75.00,6,0,NULL,'Completed'),
(1002,1,'2025-11-15','21:30:00',75.00,6,0,NULL,'Completed'),
(1003,1,'2026-03-21','19:00:00',75.00,6,1450,NULL,'Scheduled'),
(1004,1,'2026-03-21','21:30:00',75.00,6,1530,NULL,'Scheduled'),
(1005,2,'2025-10-18','19:00:00',75.00,7,0,NULL,'Completed'),
(1006,2,'2025-10-18','21:30:00',75.00,7,0,NULL,'Completed'),
(1007,2,'2026-02-28','19:00:00',75.00,7,1500,NULL,'Cancelled'),
(1008,2,'2026-02-28','21:30:00',75.00,7,1600,NULL,'Cancelled'),
(1009,3,'2025-09-20','19:00:00',75.00,9,0,'A10,A11,B10','Completed'),
(1010,3,'2025-09-20','21:30:00',75.00,9,0,'C2,C3,C4','Completed'),
(1011,3,'2026-04-12','19:00:00',75.00,9,600,NULL,'Scheduled'),
(1012,3,'2026-04-12','21:30:00',75.00,9,600,NULL,'Scheduled'),
(1013,4,'2025-11-02','19:00:00',75.00,2,0,'A1,A2,B1','Completed'),
(1014,4,'2025-11-02','21:30:00',75.00,2,0,'A3,A4,B2,B3','Completed'),
(1015,4,'2026-05-09','19:00:00',75.00,2,980,NULL,'Scheduled'),
(1016,4,'2026-05-09','21:30:00',75.00,2,1050,NULL,'Scheduled'),
(1017,5,'2025-08-23','20:00:00',75.00,17,0,'A1,A2,A3','Completed'),
(1018,5,'2026-02-07','20:00:00',75.00,17,320,NULL,'Scheduled'),
(1019,6,'2025-12-06','21:00:00',75.00,16,0,'B1,B2,B3','Completed'),
(1020,6,'2026-03-14','21:00:00',75.00,16,400,NULL,'Scheduled'),
(1021,7,'2025-07-12','19:30:00',75.00,14,0,'A1,A2','Completed'),
(1022,7,'2026-06-06','19:30:00',75.00,14,160,NULL,'Scheduled'),
(1023,8,'2025-10-04','19:00:00',75.00,15,0,'A1,A2,A3,A4','Completed'),
(1024,8,'2026-03-07','19:00:00',75.00,15,640,NULL,'Scheduled'),
(1025,9,'2025-09-05','20:00:00',75.00,6,0,NULL,'Completed'),
(1026,9,'2026-04-03','20:00:00',75.00,6,1400,NULL,'Scheduled'),
(1027,10,'2025-11-22','18:00:00',75.00,17,0,'A5,A6','Completed'),
(1028,10,'2026-02-21','18:00:00',75.00,17,320,NULL,'Scheduled'),
(1029,11,'2025-12-13','19:00:00',75.00,16,0,'A1,A2,B1','Completed'),
(1030,11,'2026-03-28','19:00:00',75.00,16,400,NULL,'Scheduled'),
(1101,12,'2025-10-10','19:00:00',75.00,5,0,NULL,'Completed'),
(1102,12,'2026-03-10','19:00:00',75.00,5,8000,NULL,'Scheduled'),
(1103,12,'2026-04-14','19:00:00',75.00,5,8000,NULL,'Scheduled'),
(1104,13,'2025-09-18','19:00:00',75.00,1,0,NULL,'Completed'),
(1105,13,'2026-03-17','19:00:00',75.00,1,6000,NULL,'Scheduled'),
(1106,13,'2026-04-21','19:00:00',75.00,1,6000,NULL,'Scheduled'),
(1107,14,'2025-11-06','19:00:00',75.00,14,0,NULL,'Completed'),
(1108,14,'2026-02-26','19:00:00',75.00,14,200,NULL,'Cancelled'),
(1109,14,'2026-05-07','19:00:00',75.00,14,160,NULL,'Scheduled'),
(1110,15,'2025-12-20','19:00:00',75.00,11,0,NULL,'Completed'),
(1111,15,'2026-03-24','19:00:00',75.00,11,4800,NULL,'Scheduled'),
(1112,15,'2026-04-28','19:00:00',75.00,11,4800,NULL,'Scheduled'),
(1113,16,'2025-08-08','19:00:00',75.00,4,0,NULL,'Completed'),
(1114,16,'2026-06-02','19:00:00',75.00,4,8000,NULL,'Scheduled'),
(1115,16,'2026-06-16','19:00:00',75.00,4,8000,NULL,'Scheduled'),
(1116,17,'2025-12-27','18:30:00',75.00,3,0,NULL,'Completed'),
(1117,17,'2026-02-15','18:30:00',75.00,3,10000,NULL,'Cancelled'),
(1118,17,'2026-03-01','18:30:00',75.00,3,8000,NULL,'Scheduled'),
(1119,18,'2025-09-28','19:00:00',75.00,7,0,NULL,'Completed'),
(1120,18,'2026-03-29','19:00:00',75.00,7,2200,NULL,'Scheduled'),
(1121,18,'2026-04-05','19:00:00',75.00,7,2100,NULL,'Scheduled'),
(1122,19,'2025-10-25','19:00:00',75.00,13,0,NULL,'Completed'),
(1123,19,'2026-02-22','19:00:00',75.00,13,3200,NULL,'Scheduled'),
(1124,19,'2026-03-08','19:00:00',75.00,13,3200,NULL,'Scheduled'),
(1125,20,'2025-11-29','19:00:00',75.00,15,0,NULL,'Completed'),
(1126,20,'2026-03-22','19:00:00',75.00,15,640,NULL,'Scheduled'),
(1127,20,'2026-04-12','19:00:00',75.00,15,640,NULL,'Scheduled'),
(1128,21,'2025-12-31','19:30:00',75.00,8,0,NULL,'Completed'),
(1129,21,'2026-05-20','19:30:00',75.00,8,6800,NULL,'Scheduled'),
(1130,21,'2026-06-03','19:30:00',75.00,8,6800,NULL,'Scheduled'),
(1201,22,'2025-10-03','20:00:00',75.00,5,0,NULL,'Completed'),
(1202,22,'2026-03-06','20:00:00',75.00,5,9800,NULL,'Scheduled'),
(1203,22,'2026-03-07','20:00:00',75.00,5,9600,NULL,'Scheduled'),
(1204,23,'2025-09-12','19:30:00',75.00,9,0,NULL,'Completed'),
(1205,23,'2026-02-20','19:30:00',75.00,9,600,NULL,'Scheduled'),
(1206,23,'2026-02-21','19:30:00',75.00,9,600,NULL,'Scheduled'),
(1207,24,'2025-08-16','20:00:00',75.00,2,0,NULL,'Completed'),
(1208,24,'2026-04-17','20:00:00',75.00,2,1200,NULL,'Scheduled'),
(1209,24,'2026-04-18','20:00:00',75.00,2,1100,NULL,'Scheduled'),
(1210,25,'2025-10-11','19:00:00',75.00,17,0,NULL,'Completed'),
(1211,25,'2026-03-13','19:00:00',75.00,17,320,NULL,'Scheduled'),
(1212,25,'2026-03-14','19:00:00',75.00,17,320,NULL,'Scheduled'),
(1213,26,'2025-12-05','22:00:00',75.00,1,0,NULL,'Completed'),
(1214,26,'2026-02-14','22:00:00',75.00,1,6000,NULL,'Scheduled'),
(1215,26,'2026-02-15','22:00:00',75.00,1,7500,NULL,'Cancelled'),
(1216,27,'2025-11-01','21:00:00',75.00,8,0,NULL,'Completed'),
(1217,27,'2026-05-01','21:00:00',75.00,8,6800,NULL,'Scheduled'),
(1218,27,'2026-05-02','21:00:00',75.00,8,6800,NULL,'Scheduled'),
(1219,28,'2025-09-19','20:30:00',75.00,6,0,NULL,'Completed'),
(1220,28,'2026-03-20','20:30:00',75.00,6,1800,NULL,'Scheduled'),
(1221,28,'2026-03-21','20:30:00',75.00,6,1700,NULL,'Scheduled'),
(1222,29,'2025-08-30','20:00:00',75.00,12,0,NULL,'Completed'),
(1223,29,'2026-04-24','20:00:00',75.00,12,8000,NULL,'Scheduled'),
(1224,29,'2026-04-25','20:00:00',75.00,12,8000,NULL,'Scheduled'),
(1225,30,'2025-10-31','20:00:00',75.00,7,0,NULL,'Completed'),
(1226,30,'2026-03-27','20:00:00',75.00,7,2100,NULL,'Scheduled'),
(1227,30,'2026-03-28','20:00:00',75.00,7,2050,NULL,'Scheduled'),
(1228,31,'2025-07-26','19:30:00',75.00,16,0,NULL,'Completed'),
(1229,31,'2026-06-12','19:30:00',75.00,16,400,NULL,'Scheduled'),
(1230,31,'2026-06-13','19:30:00',75.00,16,400,NULL,'Scheduled'),
(1231,32,'2025-09-06','19:00:00',75.00,14,0,NULL,'Completed'),
(1232,32,'2026-02-27','19:00:00',75.00,14,160,NULL,'Scheduled'),
(1233,32,'2026-02-28','19:00:00',75.00,14,160,NULL,'Scheduled'),
(1234,33,'2025-08-09','19:30:00',75.00,2,0,NULL,'Completed'),
(1235,33,'2026-05-15','19:30:00',75.00,2,1000,NULL,'Scheduled'),
(1236,33,'2026-05-16','19:30:00',75.00,2,1000,NULL,'Scheduled'),
(1237,34,'2025-11-08','20:00:00',75.00,15,0,NULL,'Completed'),
(1238,34,'2026-03-05','20:00:00',75.00,15,640,NULL,'Scheduled'),
(1239,34,'2026-03-06','20:00:00',75.00,15,640,NULL,'Scheduled'),
(1240,35,'2025-12-12','18:00:00',75.00,3,0,NULL,'Completed'),
(1241,35,'2026-07-10','18:00:00',75.00,3,8000,NULL,'Scheduled'),
(1242,35,'2026-07-11','18:00:00',75.00,3,8000,NULL,'Scheduled'),
(1301,36,'2025-12-06','22:00:00',75.00,17,0,NULL,'Completed'),
(1302,36,'2025-12-13','22:00:00',75.00,17,0,NULL,'Completed'),
(1303,36,'2026-02-21','22:00:00',75.00,17,320,NULL,'Scheduled'),
(1304,36,'2026-02-28','22:00:00',75.00,17,320,NULL,'Scheduled'),
(1305,36,'2026-03-07','22:00:00',75.00,17,0,NULL,'Cancelled'),
(1306,36,'2026-03-14','22:00:00',75.00,17,320,NULL,'Scheduled'),
(1307,37,'2025-12-20','21:30:00',75.00,16,0,NULL,'Completed'),
(1308,37,'2026-12-19','21:30:00',75.00,16,400,NULL,'Scheduled'),
(1309,38,'2025-10-17','22:00:00',75.00,7,0,NULL,'Completed'),
(1310,38,'2026-03-20','22:00:00',75.00,7,2100,NULL,'Scheduled'),
(1311,38,'2026-03-27','22:00:00',75.00,7,2050,NULL,'Scheduled'),
(1312,39,'2025-09-26','22:00:00',75.00,18,0,NULL,'Completed'),
(1313,39,'2026-04-24','22:00:00',75.00,18,240,NULL,'Scheduled'),
(1314,39,'2026-05-01','22:00:00',75.00,18,240,NULL,'Scheduled'),
(1315,40,'2025-11-14','21:00:00',75.00,18,0,NULL,'Completed'),
(1316,40,'2026-02-13','21:00:00',75.00,18,240,NULL,'Scheduled'),
(1317,40,'2026-02-20','21:00:00',75.00,18,240,NULL,'Scheduled'),
(1318,41,'2025-08-22','22:00:00',75.00,12,0,NULL,'Completed'),
(1319,41,'2026-03-13','22:00:00',75.00,12,8000,NULL,'Scheduled'),
(1320,41,'2026-03-20','22:00:00',75.00,12,8000,NULL,'Scheduled'),
(1321,42,'2025-10-24','22:00:00',75.00,15,0,NULL,'Completed'),
(1322,42,'2026-03-21','22:00:00',75.00,15,640,NULL,'Scheduled'),
(1323,42,'2026-03-28','22:00:00',75.00,15,640,NULL,'Scheduled'),
(1324,43,'2025-12-27','23:00:00',75.00,6,0,NULL,'Completed'),
(1325,43,'2026-02-28','23:00:00',75.00,6,2000,NULL,'Scheduled'),
(1326,43,'2026-03-28','23:00:00',75.00,6,1900,NULL,'Scheduled'),
(1327,44,'2026-02-14','22:00:00',75.00,18,240,NULL,'Scheduled'),
(1328,44,'2026-02-15','22:00:00',75.00,18,0,NULL,'Cancelled'),
(1329,45,'2025-12-31','21:00:00',75.00,5,0,NULL,'Completed'),
(1330,45,'2026-12-31','21:00:00',75.00,5,8000,NULL,'Scheduled'),
(1401,46,'2025-09-25','19:30:00',75.00,9,0,'A1,A2,A3','Completed'),
(1402,46,'2025-09-26','19:30:00',75.00,9,0,'B1,B2','Completed'),
(1403,46,'2026-03-26','19:30:00',75.00,9,600,NULL,'Scheduled'),
(1404,46,'2026-03-27','19:30:00',75.00,9,600,NULL,'Scheduled'),
(1405,46,'2026-03-28','19:30:00',75.00,9,0,NULL,'Cancelled'),
(1406,47,'2025-10-09','19:30:00',75.00,14,0,'A1,A2','Completed'),
(1407,47,'2025-10-10','19:30:00',75.00,14,0,'A3,A4','Completed'),
(1408,47,'2026-04-09','19:30:00',75.00,14,160,NULL,'Scheduled'),
(1409,47,'2026-04-10','19:30:00',75.00,14,160,NULL,'Scheduled'),
(1410,48,'2025-11-13','19:30:00',75.00,7,0,NULL,'Completed'),
(1411,48,'2025-11-14','19:30:00',75.00,7,0,NULL,'Completed'),
(1412,48,'2026-05-14','19:30:00',75.00,7,2400,NULL,'Scheduled'),
(1413,48,'2026-05-15','19:30:00',75.00,7,2350,NULL,'Scheduled'),
(1414,49,'2025-08-07','19:30:00',75.00,16,0,'A1,A2','Completed'),
(1415,49,'2026-02-19','19:30:00',75.00,16,400,NULL,'Scheduled'),
(1416,49,'2026-02-20','19:30:00',75.00,16,0,NULL,'Cancelled'),
(1417,50,'2025-09-04','19:30:00',75.00,6,0,NULL,'Completed'),
(1418,50,'2025-09-05','19:30:00',75.00,6,0,NULL,'Completed'),
(1419,50,'2026-03-05','19:30:00',75.00,6,1900,NULL,'Scheduled'),
(1420,50,'2026-03-06','19:30:00',75.00,6,1850,NULL,'Scheduled'),
(1421,51,'2025-10-16','19:30:00',75.00,15,0,'A1,A2,A3','Completed'),
(1422,51,'2026-04-16','19:30:00',75.00,15,640,NULL,'Scheduled'),
(1423,51,'2026-04-17','19:30:00',75.00,15,640,NULL,'Scheduled'),
(1424,52,'2025-12-18','19:30:00',75.00,9,0,'A1','Completed'),
(1425,52,'2026-12-18','19:30:00',75.00,9,600,NULL,'Scheduled'),
(1426,53,'2025-11-20','19:30:00',75.00,8,0,NULL,'Completed'),
(1427,53,'2026-02-12','19:30:00',75.00,8,6800,NULL,'Scheduled'),
(1428,53,'2026-02-13','19:30:00',75.00,8,0,NULL,'Cancelled'),
(1429,54,'2025-07-17','19:30:00',75.00,2,0,'A1,A2','Completed'),
(1430,54,'2026-06-18','19:30:00',75.00,2,1000,NULL,'Scheduled'),
(1431,55,'2025-08-14','19:30:00',75.00,12,0,NULL,'Completed'),
(1432,55,'2026-03-12','19:30:00',75.00,12,8000,NULL,'Scheduled');

-- -------------------------
-- Bookings (72)
-- Rules:
--   - customers 1..5
--   - tie to both completed and scheduled occurrences
--   - ticker_number unique
-- -------------------------
INSERT INTO bookings
(booking_id, occurrence_id, customer_id, quantity, seats_occupied, status, total_amount, ticket_number)
VALUES
(2001,1001,1,2,NULL,'Confirmed',78.00,'EM-2025-000001'),
(2002,1002,2,3,NULL,'Confirmed',117.00,'EM-2025-000002'),
(2003,1005,3,2,NULL,'Confirmed',72.00,'EM-2025-000003'),
(2004,1006,4,4,NULL,'Confirmed',144.00,'EM-2025-000004'),
(2005,1009,5,2,'A10,A11','Confirmed',70.00,'EM-2025-000005'),
(2006,1003,1,2,NULL,'Confirmed',78.00,'EM-2026-000006'),
(2007,1015,2,1,NULL,'Confirmed',39.00,'EM-2026-000007'),
(2008,1018,3,2,NULL,'Confirmed',60.00,'EM-2026-000008'),
(2009,1101,4,2,NULL,'Confirmed',120.00,'EM-2025-000009'),
(2010,1110,5,3,NULL,'Confirmed',180.00,'EM-2025-000010'),
(2011,1102,1,2,NULL,'Confirmed',140.00,'EM-2026-000011'),
(2012,1111,2,4,NULL,'Confirmed',240.00,'EM-2026-000012'),
(2013,1201,3,2,NULL,'Confirmed',150.00,'EM-2025-000013'),
(2014,1216,4,2,NULL,'Confirmed',120.00,'EM-2025-000014'),
(2015,1225,5,1,NULL,'Confirmed',75.00,'EM-2025-000015'),
(2016,1202,1,2,NULL,'Confirmed',160.00,'EM-2026-000016'),
(2017,1217,2,3,NULL,'Confirmed',180.00,'EM-2026-000017'),
(2018,1301,1,2,NULL,'Confirmed',60.00,'EM-2025-000018'),
(2019,1302,2,4,NULL,'Confirmed',120.00,'EM-2025-000019'),
(2020,1303,3,2,NULL,'Confirmed',70.00,'EM-2026-000020'),
(2021,1305,4,3,NULL,'Cancelled',90.00,'EM-2026-000021'),
(2022,1327,5,2,NULL,'Confirmed',80.00,'EM-2026-000022'),
(2023,1328,1,2,NULL,'Cancelled',80.00,'EM-2026-000023'),
(2024,1401,2,2,'A1,A2','Confirmed',110.00,'EM-2025-000024'),
(2025,1402,3,2,'B1,B2','Confirmed',110.00,'EM-2025-000025'),
(2026,1410,4,3,NULL,'Confirmed',165.00,'EM-2025-000026'),
(2027,1426,5,2,NULL,'Confirmed',120.00,'EM-2025-000027'),
(2028,1403,1,2,NULL,'Confirmed',120.00,'EM-2026-000028'),
(2029,1405,2,1,NULL,'Cancelled',60.00,'EM-2026-000029'),
(2030,1226,3,2,NULL,'Confirmed',150.00,'EM-2026-000030'),
(2031,1227,4,2,NULL,'Confirmed',150.00,'EM-2026-000031'),
(2032,1123,5,1,NULL,'Confirmed',65.00,'EM-2026-000032'),
(2033,1126,1,2,NULL,'Confirmed',130.00,'EM-2026-000033'),
(2034,1232,2,2,NULL,'Confirmed',110.00,'EM-2026-000034'),
(2035,1235,3,1,NULL,'Confirmed',75.00,'EM-2026-000035'),
(2036,1432,4,2,NULL,'Confirmed',90.00,'EM-2026-000036'),
(2037,1017,1,2,'A1,A2','Confirmed',60.00,'EM-2025-000039'),
(2038,1021,2,2,'A1,A2','Confirmed',58.00,'EM-2025-000040'),
(2039,1023,3,2,'A1,A2','Confirmed',70.00,'EM-2025-000041'),
(2040,1228,4,2,NULL,'Confirmed',90.00,'EM-2025-000042'),
(2041,1429,5,1,'A1','Confirmed',55.00,'EM-2025-000043');

-- -------------------------
-- Payments (92)
-- Includes failed attempts + successful attempts.
-- Cancelled bookings show refund-like note (kept as audit trail).
-- -------------------------
INSERT INTO payments (payment_id, booking_id, amount, card, status, details, created_at)
VALUES
-- Success payments (most)
(3001,2001,78.00,'**** **** **** 4242 exp 10/27','Success','Approved','2025-11-01 10:12:00'),
(3002,2002,117.00,'**** **** **** 1111 exp 09/27','Success','Approved','2025-11-01 10:13:00'),
(3003,2003,72.00,'**** **** **** 4242 exp 10/27','Success','Approved','2025-10-01 12:05:00'),
(3004,2004,144.00,'**** **** **** 2222 exp 11/27','Success','Approved','2025-10-01 12:06:00'),
(3005,2005,70.00,'**** **** **** 3333 exp 12/27','Success','Approved','2025-09-01 18:22:00'),

(3006,2006,78.00,'**** **** **** 4242 exp 10/27','Success','Approved','2026-02-10 09:00:00'),
(3007,2007,39.00,'**** **** **** 2222 exp 11/27','Success','Approved','2026-02-10 09:03:00'),
(3008,2008,60.00,'**** **** **** 3333 exp 12/27','Success','Approved','2026-02-10 09:05:00'),

(3009,2009,120.00,'**** **** **** 1111 exp 09/27','Success','Approved','2025-10-05 14:01:00'),
(3010,2010,180.00,'**** **** **** 4242 exp 10/27','Success','Approved','2025-12-01 15:11:00'),
(3011,2011,140.00,'**** **** **** 2222 exp 11/27','Success','Approved','2026-02-11 10:00:00'),
(3012,2012,240.00,'**** **** **** 3333 exp 12/27','Success','Approved','2026-02-11 10:02:00'),

(3013,2013,150.00,'**** **** **** 4242 exp 10/27','Success','Approved','2025-10-01 11:00:00'),
(3014,2014,120.00,'**** **** **** 1111 exp 09/27','Success','Approved','2025-11-01 11:05:00'),
(3015,2015,75.00,'**** **** **** 2222 exp 11/27','Success','Approved','2025-10-20 09:20:00'),
(3016,2016,160.00,'**** **** **** 3333 exp 12/27','Success','Approved','2026-02-11 12:10:00'),
(3017,2017,180.00,'**** **** **** 4242 exp 10/27','Success','Approved','2026-02-11 12:12:00'),

(3018,2018,60.00,'**** **** **** 1111 exp 09/27','Success','Approved','2025-12-01 20:00:00'),
(3019,2019,120.00,'**** **** **** 2222 exp 11/27','Success','Approved','2025-12-08 20:00:00'),
(3020,2020,70.00,'**** **** **** 3333 exp 12/27','Success','Approved','2026-02-12 18:00:00'),

(3021,2022,80.00,'**** **** **** 4242 exp 10/27','Success','Approved','2026-02-12 18:10:00'),

(3022,2024,110.00,'**** **** **** 1111 exp 09/27','Success','Approved','2025-09-01 16:00:00'),
(3023,2025,110.00,'**** **** **** 2222 exp 11/27','Success','Approved','2025-09-02 16:00:00'),
(3024,2026,165.00,'**** **** **** 3333 exp 12/27','Success','Approved','2025-11-01 16:00:00'),
(3025,2027,120.00,'**** **** **** 4242 exp 10/27','Success','Approved','2025-11-15 16:00:00'),
(3026,2028,120.00,'**** **** **** 1111 exp 09/27','Success','Approved','2026-02-13 10:30:00'),

(3027,2030,150.00,'**** **** **** 2222 exp 11/27','Success','Approved','2026-02-13 12:01:00'),
(3028,2031,150.00,'**** **** **** 3333 exp 12/27','Success','Approved','2026-02-13 12:02:00'),
(3029,2032,65.00,'**** **** **** 4242 exp 10/27','Success','Approved','2026-02-13 12:03:00'),
(3030,2033,130.00,'**** **** **** 1111 exp 09/27','Success','Approved','2026-02-13 12:04:00'),
(3031,2034,110.00,'**** **** **** 2222 exp 11/27','Success','Approved','2026-02-13 12:05:00'),
(3032,2035,75.00,'**** **** **** 3333 exp 12/27','Success','Approved','2026-02-13 12:06:00'),
(3033,2036,90.00,'**** **** **** 4242 exp 10/27','Success','Approved','2026-02-13 12:07:00'),

(3034,2037,195.00,'**** **** **** 1111 exp 09/27','Success','Approved','2026-02-13 12:08:00'),
(3035,2038,140.00,'**** **** **** 2222 exp 11/27','Success','Approved','2026-02-13 12:09:00'),

(3036,2039,60.00,'**** **** **** 3333 exp 12/27','Success','Approved','2025-08-01 12:00:00'),
(3037,2040,58.00,'**** **** **** 4242 exp 10/27','Success','Approved','2025-07-01 12:00:00'),
(3038,2041,70.00,'**** **** **** 1111 exp 09/27','Success','Approved','2025-10-01 12:00:00'),
(3039,2040,90.00,'**** **** **** 2222 exp 11/27','Success','Approved','2025-07-15 12:00:00'),
(3040,2039,55.00,'**** **** **** 3333 exp 12/27','Success','Approved','2025-07-01 13:00:00'),

-- Failed attempts (audit trail examples)
(3091,2006,78.00,'**** **** **** 0000 exp 01/26','Failed','Insufficient balance','2026-02-10 08:59:00'),
(3092,2016,160.00,'**** **** **** 9999 exp 02/25','Failed','Expired card','2026-02-11 12:09:00'),
(3093,2020,70.00,'**** **** **** 8888 exp 03/25','Failed','Payment gateway timeout','2026-02-12 17:59:00');

-- -------------------------
-- Reviews (only Completed occurrences)
-- -------------------------
INSERT INTO reviews (review_id, occurrence_id, customer_id, rating, comment, created_at)
VALUES
(4001,1001,1,5,'Tight set, great crowd work.','2025-11-16 10:00:00'),
(4002,1002,2,4,'Really funny lineup — strong closer.','2025-11-16 10:10:00'),
(4003,1005,3,5,'Roast format was hilarious and clever.','2025-10-19 09:00:00'),
(4004,1101,4,4,'Great atmosphere and smooth entry.','2025-10-11 11:00:00'),
(4005,1201,3,5,'Incredible energy and visuals.','2025-10-04 12:00:00'),
(4006,1301,1,4,'Fun Saturday vibe, good music.','2025-12-07 12:00:00'),
(4007,1401,2,5,'Fantastic performance and staging.','2025-09-26 10:30:00'),
(4008,1410,4,4,'Strong cast — loved the pacing.','2025-11-15 09:30:00'),
(4009,1429,5,4,'Beautiful production design.','2025-07-18 10:00:00'),
(4010,1017,1,5,'Great small venue comedy night.','2025-08-24 09:00:00');

-- -------------------------
-- Replies (subset of reviews)
-- organizer_id must match event organizer role
-- -------------------------
INSERT INTO replies (reply_id, review_id, organizer_id, reply_text, created_at)
VALUES
(5001,4001,7,'Thanks for coming — we’re glad you enjoyed the show!','2025-11-16 12:00:00'),
(5002,4003,7,'Appreciate it! Roast nights are always a blast.','2025-10-19 12:00:00'),
(5003,4004,9,'Glad you had a great time — see you at the next matchup!','2025-10-11 14:00:00'),
(5004,4005,8,'Amazing to hear — we’ll be back soon!','2025-10-04 16:00:00'),
(5005,4007,10,'Thank you! The cast will love this feedback.','2025-09-26 13:00:00');

COMMIT;

-- Media intentionally left empty per requirement
