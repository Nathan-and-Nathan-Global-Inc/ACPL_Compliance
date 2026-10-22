USE [acpl_process_valdn];
GO

/****** Object:  Table [dbo].[test_plan_mach_func_condnrel]    Script Date: 17-07-2026 17:18:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[test_plan_mach_func_condn](
	[tp_mach_fn_condn_slno] [int] IDENTITY(1,1) NOT NULL,
	[tp_hdr_slno] [int] NOT NULL,
	[test_term] [varchar](500) NOT NULL,
	[spec_requirement] [varchar](500) NOT NULL,
	[method_system] [varchar](500) NOT NULL,
	[sample_size] [int] NULL,
	[frequency] [varchar](100) NULL,
	[resp_slno] [int] NOT NULL,
	[remarks] [varchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[tp_mach_fn_condn_slno] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[test_plan_mach_func_condn]  WITH CHECK ADD  CONSTRAINT [fk_tpmachfunctioncond_tphdr] FOREIGN KEY([tp_hdr_slno])
REFERENCES [dbo].[test_plan_hdr] ([tp_hdr_slno])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[test_plan_mach_func_condn] CHECK CONSTRAINT [fk_tpmachfunctioncond_tphdr]
GO


USE [acpl_process_valdn];
GO

/****** Object:  Table [dbo].[test_plan_mach_func_final]    Script Date: 17-07-2026 17:18:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[test_plan_mach_func_final](
	[tp_mach_fn_final_slno] [int] IDENTITY(1,1) NOT NULL,
	[tp_hdr_slno] [int] NOT NULL,
	[test_term] [varchar](500) NOT NULL,
	[spec_requirement] [varchar](500) NOT NULL,
	[method_system] [varchar](500) NOT NULL,
	[sample_size] [int] NULL,
	[frequency] [varchar](100) NULL,
	[resp_slno] [int] NOT NULL,
	[remarks] [varchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[tp_mach_fn_final_slno] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[test_plan_mach_func_final]  WITH CHECK ADD  CONSTRAINT [fk_tpmachfuncfinal_tphdr] FOREIGN KEY([tp_hdr_slno])
REFERENCES [dbo].[test_plan_hdr] ([tp_hdr_slno])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[test_plan_mach_func_final] CHECK CONSTRAINT [fk_tpmachfuncfinal_tphdr]
GO




USE [acpl_process_valdn]
GO

/****** Object:  Table [dbo].[test_plan_team_members]    Script Date: 20-07-2026 19:30:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[test_plan_verifiers](
	[tp_ver_slno] [int] IDENTITY(1,1) NOT NULL,
	[tp_hdr_slno] [int] NOT NULL,
	[ver_empl_slno] [int] NOT NULL,
	verified_status varchar(20),
	verified_date datetime
PRIMARY KEY CLUSTERED 
(
	[tp_ver_slno] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


alter table test_plan_hdr add submit_status varchar(1) default 'N';
alter table test_plan_hdr add file_upload1 varchar(300);
alter table test_plan_hdr add file_upload2 varchar(300);
alter table test_plan_hdr add verified_by int;
alter table test_plan_hdr add verified_date datetime;
alter table test_plan_hdr add approved_by varchar(300);
alter table test_plan_hdr add approved_date datetime;
alter table test_plan_hdr add approved_status varchar(20);
alter table validation_hdr add file_upload varchar(300);
alter table validation_hdr add verified_status Varchar(20), approved_status Varchar(20);
alter table validation_hdr add verified_by int, verified_date datetime,approved_by int, approved_date datetime;
alter table validation_hdr add finalrel_verified_by int null, finalrel_verified_date datetime null,finalrel_approved_by int null, finalrel_approved_date datetime null,finalrel_verified_status Varchar(20) null, finalrel_approved_status Varchar(20) null;
alter table validation_hdr add finalrel_file_upload varchar(300);

-- 17-08-2026
alter table test_plan_hdr add verrejremarks varchar(500); 
alter table test_plan_hdr add apprrejremarks varchar(500); 
alter table validation_hdr add verrejremarks varchar(500); 
alter table validation_hdr add apprrejremarks varchar(500); 
alter table validation_hdr add finalrelverrejremarks varchar(500); 
alter table validation_hdr add finalrelapprrejremarks varchar(500);

-- 06-10-2026 test plan file uploads
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'test_plan_files')
BEGIN
    CREATE TABLE [dbo].[test_plan_files](
        [tp_files_slno] [int] IDENTITY(1,1) NOT NULL,
        [tp_hdr_slno] [int] NOT NULL,
        [file_for] [varchar](200) NULL,
        [file_description] [varchar](200) NULL,
        [file_name] [varchar](300) NOT NULL,
    PRIMARY KEY CLUSTERED
    (
        [tp_files_slno] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- 06-10-2026 trial file uploads
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'validation_trial_files')
BEGIN
    CREATE TABLE [dbo].[validation_trial_files](
        [val_trial_file_slno] [int] IDENTITY(1,1) NOT NULL,
        [trial_slno] [int] NOT NULL,
        [val_hdr_slno] [int] NOT NULL,
        [file_description] [varchar](200) NULL,
        [file_name] [varchar](300) NOT NULL,
    PRIMARY KEY CLUSTERED
    (
        [val_trial_file_slno] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO
 