SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='TRADITIONAL,ALLOW_INVALID_DATES';

-- -----------------------------------------------------
-- Table `user`
-- -----------------------------------------------------
DROP TABLE `user`;


-- -----------------------------------------------------
-- Table `profile`
-- -----------------------------------------------------
DROP TABLE `profile`;


-- -----------------------------------------------------
-- Table `device`
-- -----------------------------------------------------
DROP TABLE `device`;


-- -----------------------------------------------------
-- Table `notification`
-- -----------------------------------------------------
DROP TABLE `notification`;


-- -----------------------------------------------------
-- Table `role`
-- -----------------------------------------------------
DROP TABLE `role`;


-- -----------------------------------------------------
-- Table `userrole`
-- -----------------------------------------------------
DROP TABLE `userrole`;

SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;