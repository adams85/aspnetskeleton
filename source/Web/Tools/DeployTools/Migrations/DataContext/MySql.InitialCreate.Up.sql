-- -----------------------------------------------------
-- *** SCHEMA ***
-- -----------------------------------------------------

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='TRADITIONAL,ALLOW_INVALID_DATES';

-- -----------------------------------------------------
-- Table `user`
-- -----------------------------------------------------
CREATE TABLE `user` (
  `UserId` INT(11) NOT NULL AUTO_INCREMENT,
  `UserName` VARCHAR(320) NOT NULL,
  `Email` VARCHAR(320) NOT NULL,
  `Password` VARCHAR(172) NOT NULL,
  `Comment` VARCHAR(200) NULL DEFAULT NULL,
  `IsApproved` TINYINT(1) NOT NULL,
  `PasswordFailuresSinceLastSuccess` INT(11) NOT NULL,
  `LastPasswordFailureDate` DATETIME NULL DEFAULT NULL,
  `LastActivityDate` DATETIME NULL DEFAULT NULL,
  `LastLockoutDate` DATETIME NULL DEFAULT NULL,
  `LastLoginDate` DATETIME NULL DEFAULT NULL,
  `ConfirmationToken` VARCHAR(172) NULL DEFAULT NULL,
  `CreateDate` DATETIME NOT NULL,
  `IsLockedOut` TINYINT(1) NOT NULL,
  `LastPasswordChangedDate` DATETIME NOT NULL,
  `PasswordVerificationToken` VARCHAR(172) NULL DEFAULT NULL,
  `PasswordVerificationTokenExpirationDate` DATETIME NULL DEFAULT NULL,
  PRIMARY KEY (`UserId`),
  UNIQUE INDEX `IX_UserName` USING HASH (`UserName` ASC),
  UNIQUE INDEX `IX_Email` USING HASH (`Email` ASC));


-- -----------------------------------------------------
-- Table `profile`
-- -----------------------------------------------------
CREATE TABLE `profile` (
  `UserId` INT(11) NOT NULL,
  `FirstName` VARCHAR(100) NULL DEFAULT NULL,
  `LastName` VARCHAR(100) NULL DEFAULT NULL,
  `PhoneNumber` VARCHAR(50) NULL DEFAULT NULL,
  `DeviceLimit` INT(11) NOT NULL,
  PRIMARY KEY (`UserId`),
  INDEX `IX_UserId` USING HASH (`UserId` ASC),
  CONSTRAINT `FK_Profile_User_UserId`
    FOREIGN KEY (`UserId`)
    REFERENCES `user` (`UserId`)
    ON DELETE CASCADE
    ON UPDATE CASCADE);


-- -----------------------------------------------------
-- Table `device`
-- -----------------------------------------------------
CREATE TABLE `device` (
  `UserId` INT(11) NOT NULL,
  `DeviceId` VARCHAR(172) NOT NULL,
  `ConnectedAt` DATETIME NOT NULL,
  `UpdatedAt` DATETIME NOT NULL,
  `DeviceName` VARCHAR(20) NULL DEFAULT NULL,
  PRIMARY KEY (`UserId`, `DeviceId`),
  INDEX `IX_UserId` USING HASH (`UserId` ASC),
  CONSTRAINT `FK_Device_Profile_UserId`
    FOREIGN KEY (`UserId`)
    REFERENCES `profile` (`UserId`)
    ON DELETE CASCADE
    ON UPDATE CASCADE);


-- -----------------------------------------------------
-- Table `notification`
-- -----------------------------------------------------
CREATE TABLE `notification` (
  `Id` INT(11) NOT NULL AUTO_INCREMENT,
  `State` INT(11) NOT NULL,
  `CreatedAt` DATETIME NOT NULL,
  `Code` VARCHAR(64) NOT NULL,
  `Data` LONGTEXT NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `IX_CreatedAt` USING HASH (`CreatedAt` ASC));


-- -----------------------------------------------------
-- Table `role`
-- -----------------------------------------------------
CREATE TABLE `role` (
  `RoleId` INT(11) NOT NULL AUTO_INCREMENT,
  `RoleName` VARCHAR(32) NOT NULL,
  `Description` VARCHAR(256) NULL DEFAULT NULL,
  PRIMARY KEY (`RoleId`),
  UNIQUE INDEX `IX_RoleName` USING HASH (`RoleName` ASC));


-- -----------------------------------------------------
-- Table `userrole`
-- -----------------------------------------------------
CREATE TABLE `userrole` (
  `UserId` INT(11) NOT NULL,
  `RoleId` INT(11) NOT NULL,
  PRIMARY KEY (`UserId`, `RoleId`),
  INDEX `IX_UserId` USING HASH (`UserId` ASC),
  INDEX `IX_RoleId` USING HASH (`RoleId` ASC),
  CONSTRAINT `FK_UserRole_Role_RoleId`
    FOREIGN KEY (`RoleId`)
    REFERENCES `role` (`RoleId`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `FK_UserRole_User_UserId`
    FOREIGN KEY (`UserId`)
    REFERENCES `user` (`UserId`)
    ON DELETE CASCADE
    ON UPDATE CASCADE);

SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;

-- -----------------------------------------------------
-- *** DATA ***
-- -----------------------------------------------------

/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;

-- Roles, Users

LOCK TABLES `role` WRITE;
/*!40000 ALTER TABLE `role` DISABLE KEYS */;
INSERT INTO `role`
VALUES (1,'Administrators',NULL);
/*!40000 ALTER TABLE `role` ENABLE KEYS */;
UNLOCK TABLES;

LOCK TABLES `user` WRITE;
/*!40000 ALTER TABLE `user` DISABLE KEYS */;
INSERT INTO `user`
/* password: "admin" */
VALUES (1,'root','root@example.com','r4L+ezxon83leaAs/l5uhWyuxBgswAARkD3ecp/H4mcSinGq/ER50CPgKTEJo9m1',NULL,1,0,NOW(),NOW(),NOW(),NOW(),NULL,NOW(),0,NOW(),NULL,NULL);
/*!40000 ALTER TABLE `user` ENABLE KEYS */;
UNLOCK TABLES;

LOCK TABLES `profile` WRITE;
/*!40000 ALTER TABLE `profile` DISABLE KEYS */;
INSERT INTO `profile`
VALUES (1,'System','Administrator',NULL,0);
/*!40000 ALTER TABLE `profile` ENABLE KEYS */;
UNLOCK TABLES;

LOCK TABLES `userrole` WRITE;
/*!40000 ALTER TABLE `userrole` DISABLE KEYS */;
INSERT INTO `userrole` VALUES (1,1);
/*!40000 ALTER TABLE `userrole` ENABLE KEYS */;
UNLOCK TABLES;

/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;
