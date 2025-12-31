--
-- PostgreSQL database dump
--

\restrict Uq0tdwC3sF07syKaISeVFUtuObd6dLsQ7W5IRNdFzhVpv25Xu9vAkuRQJgaVyka

-- Dumped from database version 18.0
-- Dumped by pg_dump version 18.0

-- Started on 2025-12-30 21:16:41

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

ALTER TABLE IF EXISTS ONLY public."UserTenants" DROP CONSTRAINT IF EXISTS "FK_UserTenants_Tenants_TenantId";
ALTER TABLE IF EXISTS ONLY public."UserTenants" DROP CONSTRAINT IF EXISTS "FK_UserTenants_AspNetUsers_UserId";
ALTER TABLE IF EXISTS ONLY public."UserTenants" DROP CONSTRAINT IF EXISTS "FK_UserTenants_AspNetRoles_RoleId";
ALTER TABLE IF EXISTS ONLY public."UserPreferences" DROP CONSTRAINT IF EXISTS "FK_UserPreferences_AspNetUsers_UserId";
ALTER TABLE IF EXISTS ONLY public."TenantN8nConfigs" DROP CONSTRAINT IF EXISTS "FK_TenantN8nConfigs_Tenants_TenantId";
ALTER TABLE IF EXISTS ONLY public."TenantAIConfigs" DROP CONSTRAINT IF EXISTS "FK_TenantAIConfigs_Tenants_TenantId";
ALTER TABLE IF EXISTS ONLY public."PublishingJobs" DROP CONSTRAINT IF EXISTS "FK_PublishingJobs_MarketingPacks_MarketingPackId";
ALTER TABLE IF EXISTS ONLY public."PublishingJobs" DROP CONSTRAINT IF EXISTS "FK_PublishingJobs_MarketingAssetPrompts_MarketingAssetPromptId";
ALTER TABLE IF EXISTS ONLY public."PublishingJobs" DROP CONSTRAINT IF EXISTS "FK_PublishingJobs_GeneratedCopies_GeneratedCopyId";
ALTER TABLE IF EXISTS ONLY public."PublishingJobs" DROP CONSTRAINT IF EXISTS "FK_PublishingJobs_Campaigns_CampaignId";
ALTER TABLE IF EXISTS ONLY public."PublishingJobMetrics" DROP CONSTRAINT IF EXISTS "FK_PublishingJobMetrics_PublishingJobs_PublishingJobId";
ALTER TABLE IF EXISTS ONLY public."MarketingPacks" DROP CONSTRAINT IF EXISTS "FK_MarketingPacks_Contents_ContentId";
ALTER TABLE IF EXISTS ONLY public."MarketingPacks" DROP CONSTRAINT IF EXISTS "FK_MarketingPacks_Campaigns_CampaignId";
ALTER TABLE IF EXISTS ONLY public."MarketingMemories" DROP CONSTRAINT IF EXISTS "FK_MarketingMemories_Campaigns_CampaignId";
ALTER TABLE IF EXISTS ONLY public."MarketingAssetPrompts" DROP CONSTRAINT IF EXISTS "FK_MarketingAssetPrompts_MarketingPacks_MarketingPackId";
ALTER TABLE IF EXISTS ONLY public."GeneratedCopies" DROP CONSTRAINT IF EXISTS "FK_GeneratedCopies_MarketingPacks_MarketingPackId";
ALTER TABLE IF EXISTS ONLY public."Contents" DROP CONSTRAINT IF EXISTS "FK_Contents_Campaigns_CampaignId";
ALTER TABLE IF EXISTS ONLY public."Consents" DROP CONSTRAINT IF EXISTS "FK_Consents_AspNetUsers_UserId";
ALTER TABLE IF EXISTS ONLY public."Campaigns" DROP CONSTRAINT IF EXISTS "FK_Campaigns_Tenants_TenantId";
ALTER TABLE IF EXISTS ONLY public."CampaignMetrics" DROP CONSTRAINT IF EXISTS "FK_CampaignMetrics_Campaigns_CampaignId";
ALTER TABLE IF EXISTS ONLY public."CampaignDrafts" DROP CONSTRAINT IF EXISTS "FK_CampaignDrafts_MarketingPacks_MarketingPackId";
ALTER TABLE IF EXISTS ONLY public."CampaignDrafts" DROP CONSTRAINT IF EXISTS "FK_CampaignDrafts_Campaigns_ConvertedCampaignId";
ALTER TABLE IF EXISTS ONLY public."AutomationStates" DROP CONSTRAINT IF EXISTS "FK_AutomationStates_Campaigns_CampaignId";
ALTER TABLE IF EXISTS ONLY public."AspNetUsers" DROP CONSTRAINT IF EXISTS "FK_AspNetUsers_Tenants_TenantId";
ALTER TABLE IF EXISTS ONLY public."AspNetUserTokens" DROP CONSTRAINT IF EXISTS "FK_AspNetUserTokens_AspNetUsers_UserId";
ALTER TABLE IF EXISTS ONLY public."AspNetUserRoles" DROP CONSTRAINT IF EXISTS "FK_AspNetUserRoles_AspNetUsers_UserId";
ALTER TABLE IF EXISTS ONLY public."AspNetUserRoles" DROP CONSTRAINT IF EXISTS "FK_AspNetUserRoles_AspNetRoles_RoleId";
ALTER TABLE IF EXISTS ONLY public."AspNetUserLogins" DROP CONSTRAINT IF EXISTS "FK_AspNetUserLogins_AspNetUsers_UserId";
ALTER TABLE IF EXISTS ONLY public."AspNetUserClaims" DROP CONSTRAINT IF EXISTS "FK_AspNetUserClaims_AspNetUsers_UserId";
ALTER TABLE IF EXISTS ONLY public."AspNetRoleClaims" DROP CONSTRAINT IF EXISTS "FK_AspNetRoleClaims_AspNetRoles_RoleId";
DROP INDEX IF EXISTS public."UserNameIndex";
DROP INDEX IF EXISTS public."RoleNameIndex";
DROP INDEX IF EXISTS public."IX_UserTenants_UserId_TenantId";
DROP INDEX IF EXISTS public."IX_UserTenants_TenantId_RoleId";
DROP INDEX IF EXISTS public."IX_UserTenants_RoleId";
DROP INDEX IF EXISTS public."IX_UserPreferences_UserId";
DROP INDEX IF EXISTS public."IX_UserPreferences_TenantId_UserId_PreferenceKey";
DROP INDEX IF EXISTS public."IX_UserPreferences_TenantId";
DROP INDEX IF EXISTS public."IX_Tenants_Subdomain";
DROP INDEX IF EXISTS public."IX_TenantN8nConfigs_TenantId";
DROP INDEX IF EXISTS public."IX_TenantAIConfigs_TenantId";
DROP INDEX IF EXISTS public."IX_PublishingJobs_TenantId_ScheduledDate";
DROP INDEX IF EXISTS public."IX_PublishingJobs_TenantId_CampaignId_Status";
DROP INDEX IF EXISTS public."IX_PublishingJobs_TenantId";
DROP INDEX IF EXISTS public."IX_PublishingJobs_MarketingPackId";
DROP INDEX IF EXISTS public."IX_PublishingJobs_MarketingAssetPromptId";
DROP INDEX IF EXISTS public."IX_PublishingJobs_GeneratedCopyId";
DROP INDEX IF EXISTS public."IX_PublishingJobs_CampaignId";
DROP INDEX IF EXISTS public."IX_PublishingJobMetrics_TenantId_PublishingJobId_MetricDate";
DROP INDEX IF EXISTS public."IX_PublishingJobMetrics_TenantId_MetricDate";
DROP INDEX IF EXISTS public."IX_PublishingJobMetrics_TenantId";
DROP INDEX IF EXISTS public."IX_PublishingJobMetrics_PublishingJobId";
DROP INDEX IF EXISTS public."IX_MarketingPacks_TenantId_Status";
DROP INDEX IF EXISTS public."IX_MarketingPacks_TenantId_ContentId";
DROP INDEX IF EXISTS public."IX_MarketingPacks_TenantId";
DROP INDEX IF EXISTS public."IX_MarketingPacks_ContentId";
DROP INDEX IF EXISTS public."IX_MarketingPacks_CampaignId";
DROP INDEX IF EXISTS public."IX_MarketingMemories_TenantId";
DROP INDEX IF EXISTS public."IX_MarketingMemories_CampaignId";
DROP INDEX IF EXISTS public."IX_MarketingAssetPrompts_TenantId_MarketingPackId";
DROP INDEX IF EXISTS public."IX_MarketingAssetPrompts_TenantId";
DROP INDEX IF EXISTS public."IX_MarketingAssetPrompts_MarketingPackId";
DROP INDEX IF EXISTS public."IX_GeneratedCopies_TenantId_MarketingPackId";
DROP INDEX IF EXISTS public."IX_GeneratedCopies_TenantId";
DROP INDEX IF EXISTS public."IX_GeneratedCopies_MarketingPackId";
DROP INDEX IF EXISTS public."IX_Contents_TenantId";
DROP INDEX IF EXISTS public."IX_Contents_CampaignId";
DROP INDEX IF EXISTS public."IX_Consents_UserId";
DROP INDEX IF EXISTS public."IX_Consents_TenantId_UserId_ConsentType";
DROP INDEX IF EXISTS public."IX_Consents_TenantId";
DROP INDEX IF EXISTS public."IX_Campaigns_TenantId_Status";
DROP INDEX IF EXISTS public."IX_Campaigns_TenantId";
DROP INDEX IF EXISTS public."IX_CampaignMetrics_TenantId_MetricDate";
DROP INDEX IF EXISTS public."IX_CampaignMetrics_TenantId_CampaignId_MetricDate";
DROP INDEX IF EXISTS public."IX_CampaignMetrics_TenantId";
DROP INDEX IF EXISTS public."IX_CampaignMetrics_CampaignId";
DROP INDEX IF EXISTS public."IX_CampaignDrafts_TenantId_Status";
DROP INDEX IF EXISTS public."IX_CampaignDrafts_TenantId";
DROP INDEX IF EXISTS public."IX_CampaignDrafts_MarketingPackId";
DROP INDEX IF EXISTS public."IX_CampaignDrafts_ConvertedCampaignId";
DROP INDEX IF EXISTS public."IX_AutomationStates_TenantId";
DROP INDEX IF EXISTS public."IX_AutomationStates_CampaignId";
DROP INDEX IF EXISTS public."IX_AutomationExecutions_TenantId_Status";
DROP INDEX IF EXISTS public."IX_AutomationExecutions_TenantId";
DROP INDEX IF EXISTS public."IX_AutomationExecutions_RequestId";
DROP INDEX IF EXISTS public."IX_AuditLogs_TenantId_CreatedAt";
DROP INDEX IF EXISTS public."IX_AuditLogs_TenantId_Action_EntityType";
DROP INDEX IF EXISTS public."IX_AuditLogs_TenantId";
DROP INDEX IF EXISTS public."IX_AspNetUsers_TenantId_Email";
DROP INDEX IF EXISTS public."IX_AspNetUserRoles_RoleId";
DROP INDEX IF EXISTS public."IX_AspNetUserLogins_UserId";
DROP INDEX IF EXISTS public."IX_AspNetUserClaims_UserId";
DROP INDEX IF EXISTS public."IX_AspNetRoleClaims_RoleId";
DROP INDEX IF EXISTS public."EmailIndex";
ALTER TABLE IF EXISTS ONLY public."__EFMigrationsHistory" DROP CONSTRAINT IF EXISTS "PK___EFMigrationsHistory";
ALTER TABLE IF EXISTS ONLY public."UserTenants" DROP CONSTRAINT IF EXISTS "PK_UserTenants";
ALTER TABLE IF EXISTS ONLY public."UserPreferences" DROP CONSTRAINT IF EXISTS "PK_UserPreferences";
ALTER TABLE IF EXISTS ONLY public."Tenants" DROP CONSTRAINT IF EXISTS "PK_Tenants";
ALTER TABLE IF EXISTS ONLY public."TenantN8nConfigs" DROP CONSTRAINT IF EXISTS "PK_TenantN8nConfigs";
ALTER TABLE IF EXISTS ONLY public."TenantAIConfigs" DROP CONSTRAINT IF EXISTS "PK_TenantAIConfigs";
ALTER TABLE IF EXISTS ONLY public."PublishingJobs" DROP CONSTRAINT IF EXISTS "PK_PublishingJobs";
ALTER TABLE IF EXISTS ONLY public."PublishingJobMetrics" DROP CONSTRAINT IF EXISTS "PK_PublishingJobMetrics";
ALTER TABLE IF EXISTS ONLY public."MarketingPacks" DROP CONSTRAINT IF EXISTS "PK_MarketingPacks";
ALTER TABLE IF EXISTS ONLY public."MarketingMemories" DROP CONSTRAINT IF EXISTS "PK_MarketingMemories";
ALTER TABLE IF EXISTS ONLY public."MarketingAssetPrompts" DROP CONSTRAINT IF EXISTS "PK_MarketingAssetPrompts";
ALTER TABLE IF EXISTS ONLY public."GeneratedCopies" DROP CONSTRAINT IF EXISTS "PK_GeneratedCopies";
ALTER TABLE IF EXISTS ONLY public."Contents" DROP CONSTRAINT IF EXISTS "PK_Contents";
ALTER TABLE IF EXISTS ONLY public."Consents" DROP CONSTRAINT IF EXISTS "PK_Consents";
ALTER TABLE IF EXISTS ONLY public."Campaigns" DROP CONSTRAINT IF EXISTS "PK_Campaigns";
ALTER TABLE IF EXISTS ONLY public."CampaignMetrics" DROP CONSTRAINT IF EXISTS "PK_CampaignMetrics";
ALTER TABLE IF EXISTS ONLY public."CampaignDrafts" DROP CONSTRAINT IF EXISTS "PK_CampaignDrafts";
ALTER TABLE IF EXISTS ONLY public."AutomationStates" DROP CONSTRAINT IF EXISTS "PK_AutomationStates";
ALTER TABLE IF EXISTS ONLY public."AutomationExecutions" DROP CONSTRAINT IF EXISTS "PK_AutomationExecutions";
ALTER TABLE IF EXISTS ONLY public."AuditLogs" DROP CONSTRAINT IF EXISTS "PK_AuditLogs";
ALTER TABLE IF EXISTS ONLY public."AspNetUsers" DROP CONSTRAINT IF EXISTS "PK_AspNetUsers";
ALTER TABLE IF EXISTS ONLY public."AspNetUserTokens" DROP CONSTRAINT IF EXISTS "PK_AspNetUserTokens";
ALTER TABLE IF EXISTS ONLY public."AspNetUserRoles" DROP CONSTRAINT IF EXISTS "PK_AspNetUserRoles";
ALTER TABLE IF EXISTS ONLY public."AspNetUserLogins" DROP CONSTRAINT IF EXISTS "PK_AspNetUserLogins";
ALTER TABLE IF EXISTS ONLY public."AspNetUserClaims" DROP CONSTRAINT IF EXISTS "PK_AspNetUserClaims";
ALTER TABLE IF EXISTS ONLY public."AspNetRoles" DROP CONSTRAINT IF EXISTS "PK_AspNetRoles";
ALTER TABLE IF EXISTS ONLY public."AspNetRoleClaims" DROP CONSTRAINT IF EXISTS "PK_AspNetRoleClaims";
DROP TABLE IF EXISTS public."__EFMigrationsHistory";
DROP TABLE IF EXISTS public."UserTenants";
DROP TABLE IF EXISTS public."UserPreferences";
DROP TABLE IF EXISTS public."Tenants";
DROP TABLE IF EXISTS public."TenantN8nConfigs";
DROP TABLE IF EXISTS public."TenantAIConfigs";
DROP TABLE IF EXISTS public."PublishingJobs";
DROP TABLE IF EXISTS public."PublishingJobMetrics";
DROP TABLE IF EXISTS public."MarketingPacks";
DROP TABLE IF EXISTS public."MarketingMemories";
DROP TABLE IF EXISTS public."MarketingAssetPrompts";
DROP TABLE IF EXISTS public."GeneratedCopies";
DROP TABLE IF EXISTS public."Contents";
DROP TABLE IF EXISTS public."Consents";
DROP TABLE IF EXISTS public."Campaigns";
DROP TABLE IF EXISTS public."CampaignMetrics";
DROP TABLE IF EXISTS public."CampaignDrafts";
DROP TABLE IF EXISTS public."AutomationStates";
DROP TABLE IF EXISTS public."AutomationExecutions";
DROP TABLE IF EXISTS public."AuditLogs";
DROP TABLE IF EXISTS public."AspNetUsers";
DROP TABLE IF EXISTS public."AspNetUserTokens";
DROP TABLE IF EXISTS public."AspNetUserRoles";
DROP TABLE IF EXISTS public."AspNetUserLogins";
DROP TABLE IF EXISTS public."AspNetUserClaims";
DROP TABLE IF EXISTS public."AspNetRoles";
DROP TABLE IF EXISTS public."AspNetRoleClaims";
SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 232 (class 1259 OID 27612)
-- Name: AspNetRoleClaims; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AspNetRoleClaims" (
    "Id" integer NOT NULL,
    "RoleId" uuid NOT NULL,
    "ClaimType" text,
    "ClaimValue" text
);


--
-- TOC entry 231 (class 1259 OID 27611)
-- Name: AspNetRoleClaims_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."AspNetRoleClaims" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."AspNetRoleClaims_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 227 (class 1259 OID 27550)
-- Name: AspNetRoles; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AspNetRoles" (
    "Id" uuid NOT NULL,
    "Description" text,
    "IsActive" boolean NOT NULL,
    "Name" character varying(256),
    "NormalizedName" character varying(256),
    "ConcurrencyStamp" text
);


--
-- TOC entry 234 (class 1259 OID 27627)
-- Name: AspNetUserClaims; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AspNetUserClaims" (
    "Id" integer NOT NULL,
    "UserId" uuid NOT NULL,
    "ClaimType" text,
    "ClaimValue" text
);


--
-- TOC entry 233 (class 1259 OID 27626)
-- Name: AspNetUserClaims_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."AspNetUserClaims" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."AspNetUserClaims_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 235 (class 1259 OID 27641)
-- Name: AspNetUserLogins; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AspNetUserLogins" (
    "LoginProvider" text NOT NULL,
    "ProviderKey" text NOT NULL,
    "ProviderDisplayName" text,
    "UserId" uuid NOT NULL
);


--
-- TOC entry 236 (class 1259 OID 27656)
-- Name: AspNetUserRoles; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AspNetUserRoles" (
    "UserId" uuid NOT NULL,
    "RoleId" uuid NOT NULL
);


--
-- TOC entry 237 (class 1259 OID 27673)
-- Name: AspNetUserTokens; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AspNetUserTokens" (
    "UserId" uuid NOT NULL,
    "LoginProvider" text NOT NULL,
    "Name" text NOT NULL,
    "Value" text
);


--
-- TOC entry 228 (class 1259 OID 27559)
-- Name: AspNetUsers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AspNetUsers" (
    "Id" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "FullName" character varying(200) NOT NULL,
    "IsActive" boolean NOT NULL,
    "FailedLoginAttempts" integer NOT NULL,
    "LockoutEndDate" timestamp with time zone,
    "LastLoginAt" timestamp with time zone,
    "LastLoginIp" text,
    "UserName" character varying(256),
    "NormalizedUserName" character varying(256),
    "Email" character varying(256),
    "NormalizedEmail" character varying(256),
    "EmailConfirmed" boolean NOT NULL,
    "PasswordHash" text,
    "SecurityStamp" text,
    "ConcurrencyStamp" text,
    "PhoneNumber" text,
    "PhoneNumberConfirmed" boolean NOT NULL,
    "TwoFactorEnabled" boolean NOT NULL,
    "LockoutEnd" timestamp with time zone,
    "LockoutEnabled" boolean NOT NULL,
    "AccessFailedCount" integer NOT NULL
);


--
-- TOC entry 229 (class 1259 OID 27581)
-- Name: AuditLogs; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AuditLogs" (
    "Id" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "UserId" uuid,
    "Action" character varying(100) NOT NULL,
    "EntityType" character varying(100) NOT NULL,
    "EntityId" uuid,
    "OldValues" text,
    "NewValues" text,
    "IpAddress" text,
    "UserAgent" text,
    "Result" character varying(50) NOT NULL,
    "ErrorMessage" text,
    "RequestId" text,
    "AdditionalData" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsActive" boolean NOT NULL
);


--
-- TOC entry 230 (class 1259 OID 27595)
-- Name: AutomationExecutions; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AutomationExecutions" (
    "Id" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "RequestId" character varying(100) NOT NULL,
    "WorkflowId" character varying(100) NOT NULL,
    "EventType" character varying(100) NOT NULL,
    "Status" character varying(50) NOT NULL,
    "DataSent" text,
    "DataReceived" text,
    "ErrorMessage" text,
    "ErrorCode" text,
    "RetryCount" integer NOT NULL,
    "Progress" integer,
    "CurrentStep" text,
    "StartedAt" timestamp with time zone,
    "CompletedAt" timestamp with time zone,
    "LastRetryAt" timestamp with time zone,
    "UserId" uuid,
    "RelatedEntityId" uuid,
    "RelatedEntityType" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsActive" boolean NOT NULL
);


--
-- TOC entry 222 (class 1259 OID 27434)
-- Name: AutomationStates; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AutomationStates" (
    "Id" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "CampaignId" uuid,
    "AutomationType" character varying(50) NOT NULL,
    "Status" character varying(50) NOT NULL,
    "ConfigurationJson" text,
    "LastExecutionAt" timestamp with time zone,
    "NextExecutionAt" timestamp with time zone,
    "ExecutionFrequency" text,
    "LastExecutionResult" text,
    "ErrorMessage" text,
    "SuccessCount" integer NOT NULL,
    "FailureCount" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsActive" boolean NOT NULL
);


--
-- TOC entry 240 (class 1259 OID 27775)
-- Name: CampaignDrafts; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."CampaignDrafts" (
    "Id" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "MarketingPackId" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Description" text,
    "Objectives" text,
    "TargetAudience" text,
    "SuggestedChannels" text,
    "Status" character varying(50) NOT NULL,
    "IsConverted" boolean NOT NULL,
    "ConvertedCampaignId" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsActive" boolean NOT NULL
);


--
-- TOC entry 244 (class 1259 OID 27889)
-- Name: CampaignMetrics; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."CampaignMetrics" (
    "Id" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "CampaignId" uuid NOT NULL,
    "MetricDate" timestamp with time zone NOT NULL,
    "Impressions" bigint NOT NULL,
    "Clicks" bigint NOT NULL,
    "Engagement" bigint NOT NULL,
    "Likes" bigint NOT NULL,
    "Comments" bigint NOT NULL,
    "Shares" bigint NOT NULL,
    "ActivePosts" integer NOT NULL,
    "IsManualEntry" boolean NOT NULL,
    "Source" character varying(50),
    "Notes" character varying(2000),
    "Metadata" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsActive" boolean NOT NULL
);


--
-- TOC entry 221 (class 1259 OID 27395)
-- Name: Campaigns; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Campaigns" (
    "Id" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Description" character varying(1000),
    "Status" character varying(50) NOT NULL,
    "MarketingStrategy" text,
    "StartDate" timestamp with time zone,
    "EndDate" timestamp with time zone,
    "Budget" numeric(18,2),
    "SpentAmount" numeric,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsActive" boolean NOT NULL,
    "Notes" character varying(5000),
    "Objectives" character varying(2000),
    "TargetAudience" character varying(2000),
    "TargetChannels" character varying(500)
);


--
-- TOC entry 225 (class 1259 OID 27493)
-- Name: Consents; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Consents" (
    "Id" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "ConsentType" character varying(100) NOT NULL,
    "IsGranted" boolean NOT NULL,
    "GrantedAt" timestamp with time zone,
    "RevokedAt" timestamp with time zone,
    "ConsentVersion" text,
    "IpAddress" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsActive" boolean NOT NULL
);


--
-- TOC entry 223 (class 1259 OID 27454)
-- Name: Contents; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Contents" (
    "Id" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "CampaignId" uuid,
    "ContentType" character varying(50) NOT NULL,
    "FileUrl" character varying(1000) NOT NULL,
    "OriginalFileName" text,
    "FileSize" bigint,
    "MimeType" text,
    "IsAiGenerated" boolean NOT NULL,
    "Description" text,
    "Tags" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsActive" boolean NOT NULL
);


--
-- TOC entry 241 (class 1259 OID 27801)
-- Name: GeneratedCopies; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."GeneratedCopies" (
    "Id" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "MarketingPackId" uuid NOT NULL,
    "CopyType" character varying(50) NOT NULL,
    "Content" text NOT NULL,
    "Hashtags" text,
    "SuggestedChannel" text,
    "PublicationChecklist" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsActive" boolean NOT NULL
);


--
-- TOC entry 242 (class 1259 OID 27820)
-- Name: MarketingAssetPrompts; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."MarketingAssetPrompts" (
    "Id" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "MarketingPackId" uuid NOT NULL,
    "AssetType" character varying(50) NOT NULL,
    "Prompt" text NOT NULL,
    "NegativePrompt" text,
    "Parameters" text,
    "SuggestedChannel" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsActive" boolean NOT NULL
);


--
-- TOC entry 224 (class 1259 OID 27473)
-- Name: MarketingMemories; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."MarketingMemories" (
    "Id" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "CampaignId" uuid,
    "MemoryType" character varying(50) NOT NULL,
    "Content" text NOT NULL,
    "ContextJson" text,
    "Tags" text,
    "RelevanceScore" integer NOT NULL,
    "MemoryDate" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsActive" boolean NOT NULL
);


--
-- TOC entry 239 (class 1259 OID 27749)
-- Name: MarketingPacks; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."MarketingPacks" (
    "Id" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "ContentId" uuid NOT NULL,
    "CampaignId" uuid,
    "Strategy" text NOT NULL,
    "Status" character varying(50) NOT NULL,
    "Version" integer NOT NULL,
    "Metadata" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsActive" boolean NOT NULL
);


--
-- TOC entry 246 (class 1259 OID 27953)
-- Name: PublishingJobMetrics; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."PublishingJobMetrics" (
    "Id" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "PublishingJobId" uuid NOT NULL,
    "MetricDate" timestamp with time zone NOT NULL,
    "Impressions" bigint NOT NULL,
    "Clicks" bigint NOT NULL,
    "Engagement" bigint NOT NULL,
    "Likes" bigint NOT NULL,
    "Comments" bigint NOT NULL,
    "Shares" bigint NOT NULL,
    "ClickThroughRate" numeric,
    "EngagementRate" numeric,
    "IsManualEntry" boolean NOT NULL,
    "Source" character varying(50),
    "Notes" character varying(2000),
    "Metadata" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsActive" boolean NOT NULL
);


--
-- TOC entry 245 (class 1259 OID 27915)
-- Name: PublishingJobs; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."PublishingJobs" (
    "Id" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "CampaignId" uuid NOT NULL,
    "MarketingPackId" uuid,
    "GeneratedCopyId" uuid,
    "MarketingAssetPromptId" uuid,
    "Channel" character varying(50) NOT NULL,
    "Status" character varying(50) NOT NULL,
    "ScheduledDate" timestamp with time zone,
    "ProcessedAt" timestamp with time zone,
    "MaxRetries" integer NOT NULL,
    "Payload" character varying(10000),
    "DownloadUrl" character varying(2000),
    "RequiresApproval" boolean NOT NULL,
    "ApprovedAt" timestamp with time zone,
    "ApprovedBy" uuid,
    "PublishedDate" timestamp with time zone,
    "PublishedUrl" character varying(1000),
    "ExternalPostId" character varying(200),
    "Content" character varying(5000) NOT NULL,
    "Hashtags" character varying(500),
    "MediaUrl" character varying(1000),
    "ErrorMessage" character varying(2000),
    "RetryCount" integer NOT NULL,
    "Metadata" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsActive" boolean NOT NULL
);


--
-- TOC entry 243 (class 1259 OID 27854)
-- Name: TenantAIConfigs; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."TenantAIConfigs" (
    "Id" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "Provider" text NOT NULL,
    "EncryptedApiKey" text NOT NULL,
    "Model" text NOT NULL,
    "AdditionalConfig" text,
    "LastUsedAt" timestamp with time zone,
    "UsageCount" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsActive" boolean NOT NULL
);


--
-- TOC entry 247 (class 1259 OID 28011)
-- Name: TenantN8nConfigs; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."TenantN8nConfigs" (
    "Id" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "UseMock" boolean NOT NULL,
    "BaseUrl" character varying(500) NOT NULL,
    "ApiUrl" character varying(500) NOT NULL,
    "EncryptedApiKey" text,
    "DefaultWebhookUrl" character varying(500) NOT NULL,
    "WebhookUrlsJson" text NOT NULL,
    "LastUsedAt" timestamp with time zone,
    "UsageCount" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsActive" boolean NOT NULL
);


--
-- TOC entry 220 (class 1259 OID 27380)
-- Name: Tenants; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Tenants" (
    "Id" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Subdomain" character varying(100) NOT NULL,
    "ContactEmail" character varying(255) NOT NULL,
    "SubscriptionPlan" text NOT NULL,
    "SubscriptionStartDate" timestamp with time zone NOT NULL,
    "SubscriptionEndDate" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsActive" boolean NOT NULL
);


--
-- TOC entry 226 (class 1259 OID 27512)
-- Name: UserPreferences; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."UserPreferences" (
    "Id" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "PreferenceKey" character varying(100) NOT NULL,
    "PreferenceValue" text NOT NULL,
    "Category" text,
    "LastUpdated" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsActive" boolean NOT NULL
);


--
-- TOC entry 238 (class 1259 OID 27688)
-- Name: UserTenants; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."UserTenants" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "RoleId" uuid NOT NULL,
    "IsPrimary" boolean NOT NULL,
    "JoinedAt" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsActive" boolean NOT NULL
);


--
-- TOC entry 219 (class 1259 OID 27373)
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


--
-- TOC entry 5223 (class 0 OID 27612)
-- Dependencies: 232
-- Data for Name: AspNetRoleClaims; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AspNetRoleClaims" ("Id", "RoleId", "ClaimType", "ClaimValue") FROM stdin;
\.


--
-- TOC entry 5218 (class 0 OID 27550)
-- Dependencies: 227
-- Data for Name: AspNetRoles; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AspNetRoles" ("Id", "Description", "IsActive", "Name", "NormalizedName", "ConcurrencyStamp") FROM stdin;
43cf26e2-4e64-4a22-a11f-fb392e3e1cf0	Dueño del tenant, acceso total	t	Owner	OWNER	\N
5873c699-0f7f-4c87-b098-288f5441e788	Administrador del tenant	t	Admin	ADMIN	\N
0321ec50-3bfc-4fdd-afe3-ad62916bba26	Marketer, puede crear/editar campañas	t	Marketer	MARKETER	\N
b55736e8-7d74-4451-b982-9f451616f2f6	Solo lectura	t	Viewer	VIEWER	\N
c1d08b85-f197-4fcb-abfc-5f931ad9b089	Super Administrador con acceso a todos los tenants	t	SuperAdmin	SUPERADMIN	fda961bd-f69a-4221-a36e-204d1c3591a5
\.


--
-- TOC entry 5225 (class 0 OID 27627)
-- Dependencies: 234
-- Data for Name: AspNetUserClaims; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AspNetUserClaims" ("Id", "UserId", "ClaimType", "ClaimValue") FROM stdin;
91	7eed7f63-a635-42c7-969d-50edf398c934	FullName	Marketer de Prueba
92	7eed7f63-a635-42c7-969d-50edf398c934	TenantId	eabb1423-ca22-4f96-817d-d068a5c5fd5f
93	532b8976-25e8-4f84-953e-289cec40aebf	FullName	Administrador de Prueba
94	532b8976-25e8-4f84-953e-289cec40aebf	TenantId	00000000-0000-0000-0000-000000000000
95	532b8976-25e8-4f84-953e-289cec40aebf	IsSuperAdmin	true
\.


--
-- TOC entry 5226 (class 0 OID 27641)
-- Dependencies: 235
-- Data for Name: AspNetUserLogins; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AspNetUserLogins" ("LoginProvider", "ProviderKey", "ProviderDisplayName", "UserId") FROM stdin;
\.


--
-- TOC entry 5227 (class 0 OID 27656)
-- Dependencies: 236
-- Data for Name: AspNetUserRoles; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AspNetUserRoles" ("UserId", "RoleId") FROM stdin;
532b8976-25e8-4f84-953e-289cec40aebf	43cf26e2-4e64-4a22-a11f-fb392e3e1cf0
7eed7f63-a635-42c7-969d-50edf398c934	0321ec50-3bfc-4fdd-afe3-ad62916bba26
532b8976-25e8-4f84-953e-289cec40aebf	c1d08b85-f197-4fcb-abfc-5f931ad9b089
\.


--
-- TOC entry 5228 (class 0 OID 27673)
-- Dependencies: 237
-- Data for Name: AspNetUserTokens; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AspNetUserTokens" ("UserId", "LoginProvider", "Name", "Value") FROM stdin;
\.


--
-- TOC entry 5219 (class 0 OID 27559)
-- Dependencies: 228
-- Data for Name: AspNetUsers; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AspNetUsers" ("Id", "TenantId", "FullName", "IsActive", "FailedLoginAttempts", "LockoutEndDate", "LastLoginAt", "LastLoginIp", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") FROM stdin;
7eed7f63-a635-42c7-969d-50edf398c934	eabb1423-ca22-4f96-817d-d068a5c5fd5f	Marketer de Prueba	t	0	\N	2025-12-30 12:06:04.791087-08	::1	marketer@test.com	MARKETER@TEST.COM	marketer@test.com	MARKETER@TEST.COM	t	AQAAAAIAAYagAAAAEEe4mn9F6a+e6QNZHkt6tVaDFJRzbg4u+kMdKNxRRLPQbMasJs09IiDnuErYA46gvA==	2UQKV4R342O5BS7YOPVBTYXKNIJ57WZS	ba1faabb-0d33-4714-aed9-bb40fac30547	\N	f	f	\N	t	0
532b8976-25e8-4f84-953e-289cec40aebf	00000000-0000-0000-0000-000000000000	Administrador de Prueba	t	0	\N	2025-12-30 16:48:52.270664-08	::1	admin@test.com	ADMIN@TEST.COM	admin@test.com	ADMIN@TEST.COM	t	AQAAAAIAAYagAAAAEJMc6fYVDUdSZTI3huMkVpRbEUAkK8rUb1nPFI0y+5nPi9WADkenBo73uqhGoWruWQ==	HZL26GARM2BPPLQQNKZHS4DJYOXILSEP	f402a4e3-6b13-4be9-9a1c-74d9468d9986	\N	f	f	\N	t	0
\.


--
-- TOC entry 5220 (class 0 OID 27581)
-- Dependencies: 229
-- Data for Name: AuditLogs; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AuditLogs" ("Id", "TenantId", "UserId", "Action", "EntityType", "EntityId", "OldValues", "NewValues", "IpAddress", "UserAgent", "Result", "ErrorMessage", "RequestId", "AdditionalData", "CreatedAt", "UpdatedAt", "IsActive") FROM stdin;
4bcf0f61-420a-490e-9d2b-09d1b0523328	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36	Success	\N	0HNI73LVGDNJ2:00000001	\N	2025-12-29 13:02:08.551077-08	\N	t
e173f05e-2536-4e48-a3b7-c9da4400ac26	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36	Success	\N	0HNI73M8NRINU:00000001	\N	2025-12-29 13:04:47.54084-08	\N	t
e6fc5a45-1217-4bcb-a6ee-2480ad230226	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36	Success	\N	0HNI73QAJHC1J:00000001	\N	2025-12-29 13:10:03.073264-08	\N	t
dde93e7e-69fe-4b7d-b166-1d6b720314c0	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36	Success	\N	0HNI73QN821NF:00000001	\N	2025-12-29 13:10:31.744086-08	\N	t
4b378538-9c1f-4630-8e53-4909c5d7f9b0	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36	Success	\N	0HNI73V2BTM0A:00000001	\N	2025-12-29 13:18:14.345965-08	\N	t
74b05bcd-82b7-408d-9e1d-6b8c490af796	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36	Success	\N	0HNI741DODEJQ:00000001	\N	2025-12-29 13:22:27.309693-08	\N	t
f5c1d78c-2d37-4759-8cb7-2fad5618b0bf	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36	Success	\N	0HNI743C1GN4C:00000001	\N	2025-12-29 13:25:56.17398-08	\N	t
d44cccfc-4f65-48af-ae86-1f02b3113892	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36	Success	\N	0HNI7463GAAHH:0000000D	\N	2025-12-29 13:30:48.84117-08	\N	t
ce8c5dbd-41b1-4d1e-8658-5a21e406bc7c	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36	Success	\N	0HNI747G721CI:0000000B	\N	2025-12-29 13:33:19.909398-08	\N	t
89917a68-80b4-49dd-995f-285be0ae2f4f	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36	Success	\N	0HNI747G721CI:00000015	\N	2025-12-29 13:33:27.39859-08	\N	t
7be9e6e3-de2b-42e0-8af1-4819e21d213f	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36	Success	\N	0HNI747G721CI:0000001F	\N	2025-12-29 13:33:28.922181-08	\N	t
263a0941-9cda-4bbc-9387-0babcb2a64db	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36	Success	\N	0HNI747G721CI:00000029	\N	2025-12-29 13:33:30.22965-08	\N	t
70bfba87-504d-41f3-8c15-c1ff6b23d990	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36	Success	\N	0HNI7480TSSKR:0000000B	\N	2025-12-29 13:34:15.027551-08	\N	t
5e800c89-b8f1-4e5b-bea9-65337fb35b9c	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36	Success	\N	0HNI7480TSSKR:00000015	\N	2025-12-29 13:34:16.8303-08	\N	t
07055c93-b19c-4428-90ea-cd67bc42ac93	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36	Success	\N	0HNI7480TSSKR:0000001F	\N	2025-12-29 13:34:25.933554-08	\N	t
661bdafb-4c51-4e10-90e5-480179630dc3	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36	Success	\N	0HNI7480TSSKR:00000029	\N	2025-12-29 13:34:27.043103-08	\N	t
43f181cc-c4ad-45f2-9ede-02f7b7bb4dd7	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT; Windows NT 10.0; es-MX) WindowsPowerShell/5.1.22000.2538	Success	\N	0HNI7B8N044IM:00000003	\N	2025-12-29 20:17:46.679704-08	\N	t
0dcad3a8-72e3-4378-aa39-9d2fa31efd44	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Cursor/2.1.39 Chrome/138.0.7204.251 Electron/37.7.0 Safari/537.36	Success	\N	0HNI7B8N044IN:00000004	\N	2025-12-29 20:22:30.208425-08	\N	t
bc4cb40a-20a7-4a1f-88d4-d098410c11f3	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Cursor/2.1.39 Chrome/138.0.7204.251 Electron/37.7.0 Safari/537.36	Success	\N	0HNI7B8N044IQ:00000002	\N	2025-12-29 20:26:35.799138-08	\N	t
bf7c9996-6a7c-4a09-985a-dd75c8bb86b1	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Cursor/2.1.39 Chrome/138.0.7204.251 Electron/37.7.0 Safari/537.36	Success	\N	0HNI7B8N044IU:00000002	\N	2025-12-29 21:45:44.430211-08	\N	t
6832248b-244c-46c3-928d-97137d3d94a8	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Cursor/2.1.39 Chrome/138.0.7204.251 Electron/37.7.0 Safari/537.36	Success	\N	0HNI7K7R8MSKL:00000004	\N	2025-12-30 04:50:19.885696-08	\N	t
2f87a7eb-6701-4b8c-ab87-005b6dc29979	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Cursor/2.1.39 Chrome/138.0.7204.251 Electron/37.7.0 Safari/537.36	Success	\N	0HNI7KAF5ETQE:00000003	\N	2025-12-30 04:55:41.167176-08	\N	t
4c87fc07-01bc-4193-b0b4-512cbcd9f06f	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Cursor/2.1.39 Chrome/138.0.7204.251 Electron/37.7.0 Safari/537.36	Success	\N	0HNI7NATV2KTE:00000003	\N	2025-12-30 07:47:36.079175-08	\N	t
adc348ac-fa20-4709-b3f1-6028a79d7382	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Cursor/2.1.39 Chrome/138.0.7204.251 Electron/37.7.0 Safari/537.36	Success	\N	0HNI7NATV2KU0:00000001	\N	2025-12-30 09:26:57.299365-08	\N	t
d53f845f-bee1-4037-a61d-c59cf4bf8830	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36	Success	\N	0HNI7P52QFHUS:0000000B	\N	2025-12-30 09:31:34.235513-08	\N	t
f10b0815-abc7-44d9-b5fa-a13b5d6b2cde	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Cursor/2.1.39 Chrome/138.0.7204.251 Electron/37.7.0 Safari/537.36	Success	\N	0HNI7Q6DLSHHC:00000003	\N	2025-12-30 10:31:18.708325-08	\N	t
f7731bdc-ee84-4b3a-8559-0f87aeee08a6	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT; Windows NT 10.0; es-MX) WindowsPowerShell/5.1.22000.2538	Success	\N	0HNI7R93VQVGQ:00000002	\N	2025-12-30 11:33:17.453925-08	\N	t
7ea61e36-037d-4127-a128-210d075386aa	eabb1423-ca22-4f96-817d-d068a5c5fd5f	\N	Login	User	7eed7f63-a635-42c7-969d-50edf398c934	\N	\N	::1	Mozilla/5.0 (Windows NT; Windows NT 10.0; es-MX) WindowsPowerShell/5.1.22000.2538	Success	\N	0HNI7R93VQVGR:00000002	\N	2025-12-30 11:33:38.568263-08	\N	t
69c6f02d-d23a-42ea-aeb3-43301dd2f4ec	eabb1423-ca22-4f96-817d-d068a5c5fd5f	\N	Login	User	7eed7f63-a635-42c7-969d-50edf398c934	\N	\N	::1	Mozilla/5.0 (Windows NT; Windows NT 10.0; es-MX) WindowsPowerShell/5.1.22000.2538	Success	\N	0HNI7R93VQVGS:00000002	\N	2025-12-30 11:33:59.770825-08	\N	t
be2a664e-8de4-4de4-b111-632093bbdd4f	eabb1423-ca22-4f96-817d-d068a5c5fd5f	\N	Login	User	7eed7f63-a635-42c7-969d-50edf398c934	\N	\N	::1	Mozilla/5.0 (Windows NT; Windows NT 10.0; es-MX) WindowsPowerShell/5.1.22000.2538	Success	\N	0HNI7RQ3VIUQ4:00000002	\N	2025-12-30 12:03:51.395282-08	\N	t
0ed2cb59-2d69-4188-b19a-673572a559d1	eabb1423-ca22-4f96-817d-d068a5c5fd5f	\N	Login	User	7eed7f63-a635-42c7-969d-50edf398c934	\N	\N	::1	Mozilla/5.0 (Windows NT; Windows NT 10.0; es-MX) WindowsPowerShell/5.1.22000.2538	Success	\N	0HNI7RRBKM0V6:00000002	\N	2025-12-30 12:06:04.965513-08	\N	t
33df3c0e-3eda-49b2-a844-f9fb1efcc593	00000000-0000-0000-0000-000000000000	\N	Login	User	532b8976-25e8-4f84-953e-289cec40aebf	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36	Success	\N	0HNI80PDRF8FV:0000000D	\N	2025-12-30 16:48:52.541028-08	\N	t
\.


--
-- TOC entry 5221 (class 0 OID 27595)
-- Dependencies: 230
-- Data for Name: AutomationExecutions; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AutomationExecutions" ("Id", "TenantId", "RequestId", "WorkflowId", "EventType", "Status", "DataSent", "DataReceived", "ErrorMessage", "ErrorCode", "RetryCount", "Progress", "CurrentStep", "StartedAt", "CompletedAt", "LastRetryAt", "UserId", "RelatedEntityId", "RelatedEntityType", "CreatedAt", "UpdatedAt", "IsActive") FROM stdin;
\.


--
-- TOC entry 5213 (class 0 OID 27434)
-- Dependencies: 222
-- Data for Name: AutomationStates; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AutomationStates" ("Id", "TenantId", "CampaignId", "AutomationType", "Status", "ConfigurationJson", "LastExecutionAt", "NextExecutionAt", "ExecutionFrequency", "LastExecutionResult", "ErrorMessage", "SuccessCount", "FailureCount", "CreatedAt", "UpdatedAt", "IsActive") FROM stdin;
\.


--
-- TOC entry 5231 (class 0 OID 27775)
-- Dependencies: 240
-- Data for Name: CampaignDrafts; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."CampaignDrafts" ("Id", "TenantId", "UserId", "MarketingPackId", "Name", "Description", "Objectives", "TargetAudience", "SuggestedChannels", "Status", "IsConverted", "ConvertedCampaignId", "CreatedAt", "UpdatedAt", "IsActive") FROM stdin;
\.


--
-- TOC entry 5235 (class 0 OID 27889)
-- Dependencies: 244
-- Data for Name: CampaignMetrics; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."CampaignMetrics" ("Id", "TenantId", "CampaignId", "MetricDate", "Impressions", "Clicks", "Engagement", "Likes", "Comments", "Shares", "ActivePosts", "IsManualEntry", "Source", "Notes", "Metadata", "CreatedAt", "UpdatedAt", "IsActive") FROM stdin;
\.


--
-- TOC entry 5212 (class 0 OID 27395)
-- Dependencies: 221
-- Data for Name: Campaigns; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Campaigns" ("Id", "TenantId", "Name", "Description", "Status", "MarketingStrategy", "StartDate", "EndDate", "Budget", "SpentAmount", "CreatedAt", "UpdatedAt", "IsActive", "Notes", "Objectives", "TargetAudience", "TargetChannels") FROM stdin;
73f24df7-644a-4895-865b-0a507926b97e	9629f563-c0b6-4570-816e-cdfb0d226167	swswswsws	wswswsws	Active	\N	2025-12-29 16:00:00-08	2026-05-28 17:00:00-07	11111.00	\N	2025-12-30 13:45:55.047344-08	\N	t	swswswsws	{"goals":["11","11","Campa\\u00F1a"]}	{"ageRange":"18-35","interests":["tecnilogia","maketing"]}	["instagram","facebook","tiktok"]
\.


--
-- TOC entry 5216 (class 0 OID 27493)
-- Dependencies: 225
-- Data for Name: Consents; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Consents" ("Id", "TenantId", "UserId", "ConsentType", "IsGranted", "GrantedAt", "RevokedAt", "ConsentVersion", "IpAddress", "CreatedAt", "UpdatedAt", "IsActive") FROM stdin;
\.


--
-- TOC entry 5214 (class 0 OID 27454)
-- Dependencies: 223
-- Data for Name: Contents; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Contents" ("Id", "TenantId", "CampaignId", "ContentType", "FileUrl", "OriginalFileName", "FileSize", "MimeType", "IsAiGenerated", "Description", "Tags", "CreatedAt", "UpdatedAt", "IsActive") FROM stdin;
\.


--
-- TOC entry 5232 (class 0 OID 27801)
-- Dependencies: 241
-- Data for Name: GeneratedCopies; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."GeneratedCopies" ("Id", "TenantId", "MarketingPackId", "CopyType", "Content", "Hashtags", "SuggestedChannel", "PublicationChecklist", "CreatedAt", "UpdatedAt", "IsActive") FROM stdin;
\.


--
-- TOC entry 5233 (class 0 OID 27820)
-- Dependencies: 242
-- Data for Name: MarketingAssetPrompts; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."MarketingAssetPrompts" ("Id", "TenantId", "MarketingPackId", "AssetType", "Prompt", "NegativePrompt", "Parameters", "SuggestedChannel", "CreatedAt", "UpdatedAt", "IsActive") FROM stdin;
\.


--
-- TOC entry 5215 (class 0 OID 27473)
-- Dependencies: 224
-- Data for Name: MarketingMemories; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."MarketingMemories" ("Id", "TenantId", "CampaignId", "MemoryType", "Content", "ContextJson", "Tags", "RelevanceScore", "MemoryDate", "CreatedAt", "UpdatedAt", "IsActive") FROM stdin;
\.


--
-- TOC entry 5230 (class 0 OID 27749)
-- Dependencies: 239
-- Data for Name: MarketingPacks; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."MarketingPacks" ("Id", "TenantId", "UserId", "ContentId", "CampaignId", "Strategy", "Status", "Version", "Metadata", "CreatedAt", "UpdatedAt", "IsActive") FROM stdin;
\.


--
-- TOC entry 5237 (class 0 OID 27953)
-- Dependencies: 246
-- Data for Name: PublishingJobMetrics; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."PublishingJobMetrics" ("Id", "TenantId", "PublishingJobId", "MetricDate", "Impressions", "Clicks", "Engagement", "Likes", "Comments", "Shares", "ClickThroughRate", "EngagementRate", "IsManualEntry", "Source", "Notes", "Metadata", "CreatedAt", "UpdatedAt", "IsActive") FROM stdin;
\.


--
-- TOC entry 5236 (class 0 OID 27915)
-- Dependencies: 245
-- Data for Name: PublishingJobs; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."PublishingJobs" ("Id", "TenantId", "CampaignId", "MarketingPackId", "GeneratedCopyId", "MarketingAssetPromptId", "Channel", "Status", "ScheduledDate", "ProcessedAt", "MaxRetries", "Payload", "DownloadUrl", "RequiresApproval", "ApprovedAt", "ApprovedBy", "PublishedDate", "PublishedUrl", "ExternalPostId", "Content", "Hashtags", "MediaUrl", "ErrorMessage", "RetryCount", "Metadata", "CreatedAt", "UpdatedAt", "IsActive") FROM stdin;
\.


--
-- TOC entry 5234 (class 0 OID 27854)
-- Dependencies: 243
-- Data for Name: TenantAIConfigs; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."TenantAIConfigs" ("Id", "TenantId", "Provider", "EncryptedApiKey", "Model", "AdditionalConfig", "LastUsedAt", "UsageCount", "CreatedAt", "UpdatedAt", "IsActive") FROM stdin;
\.


--
-- TOC entry 5238 (class 0 OID 28011)
-- Dependencies: 247
-- Data for Name: TenantN8nConfigs; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."TenantN8nConfigs" ("Id", "TenantId", "UseMock", "BaseUrl", "ApiUrl", "EncryptedApiKey", "DefaultWebhookUrl", "WebhookUrlsJson", "LastUsedAt", "UsageCount", "CreatedAt", "UpdatedAt", "IsActive") FROM stdin;
daba0ea6-143a-4e4e-93d6-087172200af1	eabb1423-ca22-4f96-817d-d068a5c5fd5f	f	https://n8n.bashpty.com	https://n8n.bashpty.com/api/v1	\N	https://n8n.bashpty.com/webhook	{"MarketingRequest": "https://n8n.bashpty.com/webhook-test/marketing-request"}	\N	0	2025-12-30 07:51:29.928033-08	2025-12-30 08:33:44.334555-08	t
\.


--
-- TOC entry 5211 (class 0 OID 27380)
-- Dependencies: 220
-- Data for Name: Tenants; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Tenants" ("Id", "Name", "Subdomain", "ContactEmail", "SubscriptionPlan", "SubscriptionStartDate", "SubscriptionEndDate", "CreatedAt", "UpdatedAt", "IsActive") FROM stdin;
eabb1423-ca22-4f96-817d-d068a5c5fd5f	Tenant de Prueba	test	test@example.com	Free	2025-12-28 08:50:25.054324-08	\N	2025-12-28 08:50:25.161125-08	\N	t
00000000-0000-0000-0000-000000000000	Super Admin Tenant	superadmin	admin@autonomousmarketing.com	Enterprise	2025-12-28 14:20:19.168075-08	\N	2025-12-28 14:20:19.168075-08	2025-12-28 14:20:19.168075-08	t
9629f563-c0b6-4570-816e-cdfb0d226167	Empresa ABC	empresa-abc	contacto@empresa-abc.com	Free	2025-12-30 05:22:17.866771-08	\N	2025-12-30 05:22:17.866771-08	\N	t
e27ce4e4-aa47-4705-a621-dca69f36c0a3	CorporaciÃ³n XYZ	corporacion-xyz	info@corporacion-xyz.com	Free	2025-12-30 05:22:17.866771-08	\N	2025-12-30 05:22:17.866771-08	\N	t
b67948ef-c256-4420-b538-e571eb09f369	Startup Tech	startup-tech	hello@startup-tech.com	Free	2025-12-30 05:22:17.866771-08	\N	2025-12-30 05:22:17.866771-08	\N	t
\.


--
-- TOC entry 5217 (class 0 OID 27512)
-- Dependencies: 226
-- Data for Name: UserPreferences; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."UserPreferences" ("Id", "TenantId", "UserId", "PreferenceKey", "PreferenceValue", "Category", "LastUpdated", "CreatedAt", "UpdatedAt", "IsActive") FROM stdin;
\.


--
-- TOC entry 5229 (class 0 OID 27688)
-- Dependencies: 238
-- Data for Name: UserTenants; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."UserTenants" ("Id", "UserId", "TenantId", "RoleId", "IsPrimary", "JoinedAt", "CreatedAt", "UpdatedAt", "IsActive") FROM stdin;
e6c04dea-1372-466c-be90-92210e00c03c	532b8976-25e8-4f84-953e-289cec40aebf	eabb1423-ca22-4f96-817d-d068a5c5fd5f	43cf26e2-4e64-4a22-a11f-fb392e3e1cf0	t	2025-12-28 08:50:25.509369-08	2025-12-28 08:50:25.568457-08	\N	t
67a035d2-a5ac-44f3-8e32-5756c3174070	7eed7f63-a635-42c7-969d-50edf398c934	eabb1423-ca22-4f96-817d-d068a5c5fd5f	0321ec50-3bfc-4fdd-afe3-ad62916bba26	t	2025-12-28 08:50:25.692797-08	2025-12-28 08:50:25.729495-08	\N	t
\.


--
-- TOC entry 5210 (class 0 OID 27373)
-- Dependencies: 219
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20251228110945_InitialCreate	8.0.2
20251228143159_AddIdentityAuthentication	8.0.2
20251228145403_AddMarketingPackTables	8.0.2
20251228150842_AddTenantAIConfig	8.0.2
20251228164127_AddMetricsTables	8.0.2
20251228172035_RemoveCampaignId1FromMarketingPacks	8.0.2
20251228174139_FixMarketingPackCampaignRelationship	8.0.2
20251228222419_UpdateConsentAndUserPreferenceToAspNetUsers	8.0.2
20251228222719_RemoveUserTable	8.0.2
20250101000000_AddTenantN8nConfig	8.0.2
\.


--
-- TOC entry 5244 (class 0 OID 0)
-- Dependencies: 231
-- Name: AspNetRoleClaims_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."AspNetRoleClaims_Id_seq"', 1, false);


--
-- TOC entry 5245 (class 0 OID 0)
-- Dependencies: 233
-- Name: AspNetUserClaims_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."AspNetUserClaims_Id_seq"', 95, true);


--
-- TOC entry 4966 (class 2606 OID 27620)
-- Name: AspNetRoleClaims PK_AspNetRoleClaims; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetRoleClaims"
    ADD CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id");


--
-- TOC entry 4947 (class 2606 OID 27558)
-- Name: AspNetRoles PK_AspNetRoles; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetRoles"
    ADD CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id");


--
-- TOC entry 4969 (class 2606 OID 27635)
-- Name: AspNetUserClaims PK_AspNetUserClaims; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserClaims"
    ADD CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id");


--
-- TOC entry 4972 (class 2606 OID 27650)
-- Name: AspNetUserLogins PK_AspNetUserLogins; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserLogins"
    ADD CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey");


--
-- TOC entry 4975 (class 2606 OID 27662)
-- Name: AspNetUserRoles PK_AspNetUserRoles; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId");


--
-- TOC entry 4977 (class 2606 OID 27682)
-- Name: AspNetUserTokens PK_AspNetUserTokens; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserTokens"
    ADD CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name");


--
-- TOC entry 4952 (class 2606 OID 27575)
-- Name: AspNetUsers PK_AspNetUsers; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUsers"
    ADD CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id");


--
-- TOC entry 4958 (class 2606 OID 27594)
-- Name: AuditLogs PK_AuditLogs; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AuditLogs"
    ADD CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Id");


--
-- TOC entry 4963 (class 2606 OID 27610)
-- Name: AutomationExecutions PK_AutomationExecutions; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AutomationExecutions"
    ADD CONSTRAINT "PK_AutomationExecutions" PRIMARY KEY ("Id");


--
-- TOC entry 4927 (class 2606 OID 27448)
-- Name: AutomationStates PK_AutomationStates; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AutomationStates"
    ADD CONSTRAINT "PK_AutomationStates" PRIMARY KEY ("Id");


--
-- TOC entry 4995 (class 2606 OID 27790)
-- Name: CampaignDrafts PK_CampaignDrafts; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."CampaignDrafts"
    ADD CONSTRAINT "PK_CampaignDrafts" PRIMARY KEY ("Id");


--
-- TOC entry 5014 (class 2606 OID 27909)
-- Name: CampaignMetrics PK_CampaignMetrics; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."CampaignMetrics"
    ADD CONSTRAINT "PK_CampaignMetrics" PRIMARY KEY ("Id");


--
-- TOC entry 4923 (class 2606 OID 27407)
-- Name: Campaigns PK_Campaigns; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Campaigns"
    ADD CONSTRAINT "PK_Campaigns" PRIMARY KEY ("Id");


--
-- TOC entry 4940 (class 2606 OID 27506)
-- Name: Consents PK_Consents; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Consents"
    ADD CONSTRAINT "PK_Consents" PRIMARY KEY ("Id");


--
-- TOC entry 4931 (class 2606 OID 27467)
-- Name: Contents PK_Contents; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Contents"
    ADD CONSTRAINT "PK_Contents" PRIMARY KEY ("Id");


--
-- TOC entry 5000 (class 2606 OID 27814)
-- Name: GeneratedCopies PK_GeneratedCopies; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."GeneratedCopies"
    ADD CONSTRAINT "PK_GeneratedCopies" PRIMARY KEY ("Id");


--
-- TOC entry 5005 (class 2606 OID 27833)
-- Name: MarketingAssetPrompts PK_MarketingAssetPrompts; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."MarketingAssetPrompts"
    ADD CONSTRAINT "PK_MarketingAssetPrompts" PRIMARY KEY ("Id");


--
-- TOC entry 4935 (class 2606 OID 27487)
-- Name: MarketingMemories PK_MarketingMemories; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."MarketingMemories"
    ADD CONSTRAINT "PK_MarketingMemories" PRIMARY KEY ("Id");


--
-- TOC entry 4989 (class 2606 OID 27764)
-- Name: MarketingPacks PK_MarketingPacks; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."MarketingPacks"
    ADD CONSTRAINT "PK_MarketingPacks" PRIMARY KEY ("Id");


--
-- TOC entry 5029 (class 2606 OID 27972)
-- Name: PublishingJobMetrics PK_PublishingJobMetrics; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PublishingJobMetrics"
    ADD CONSTRAINT "PK_PublishingJobMetrics" PRIMARY KEY ("Id");


--
-- TOC entry 5023 (class 2606 OID 27932)
-- Name: PublishingJobs PK_PublishingJobs; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PublishingJobs"
    ADD CONSTRAINT "PK_PublishingJobs" PRIMARY KEY ("Id");


--
-- TOC entry 5008 (class 2606 OID 27868)
-- Name: TenantAIConfigs PK_TenantAIConfigs; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."TenantAIConfigs"
    ADD CONSTRAINT "PK_TenantAIConfigs" PRIMARY KEY ("Id");


--
-- TOC entry 5032 (class 2606 OID 28027)
-- Name: TenantN8nConfigs PK_TenantN8nConfigs; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."TenantN8nConfigs"
    ADD CONSTRAINT "PK_TenantN8nConfigs" PRIMARY KEY ("Id");


--
-- TOC entry 4919 (class 2606 OID 27394)
-- Name: Tenants PK_Tenants; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Tenants"
    ADD CONSTRAINT "PK_Tenants" PRIMARY KEY ("Id");


--
-- TOC entry 4945 (class 2606 OID 27526)
-- Name: UserPreferences PK_UserPreferences; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserPreferences"
    ADD CONSTRAINT "PK_UserPreferences" PRIMARY KEY ("Id");


--
-- TOC entry 4982 (class 2606 OID 27700)
-- Name: UserTenants PK_UserTenants; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserTenants"
    ADD CONSTRAINT "PK_UserTenants" PRIMARY KEY ("Id");


--
-- TOC entry 4916 (class 2606 OID 27379)
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- TOC entry 4949 (class 1259 OID 27721)
-- Name: EmailIndex; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "EmailIndex" ON public."AspNetUsers" USING btree ("NormalizedEmail");


--
-- TOC entry 4964 (class 1259 OID 27716)
-- Name: IX_AspNetRoleClaims_RoleId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON public."AspNetRoleClaims" USING btree ("RoleId");


--
-- TOC entry 4967 (class 1259 OID 27718)
-- Name: IX_AspNetUserClaims_UserId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_AspNetUserClaims_UserId" ON public."AspNetUserClaims" USING btree ("UserId");


--
-- TOC entry 4970 (class 1259 OID 27719)
-- Name: IX_AspNetUserLogins_UserId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_AspNetUserLogins_UserId" ON public."AspNetUserLogins" USING btree ("UserId");


--
-- TOC entry 4973 (class 1259 OID 27720)
-- Name: IX_AspNetUserRoles_RoleId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_AspNetUserRoles_RoleId" ON public."AspNetUserRoles" USING btree ("RoleId");


--
-- TOC entry 4950 (class 1259 OID 27722)
-- Name: IX_AspNetUsers_TenantId_Email; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_AspNetUsers_TenantId_Email" ON public."AspNetUsers" USING btree ("TenantId", "Email");


--
-- TOC entry 4954 (class 1259 OID 27724)
-- Name: IX_AuditLogs_TenantId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_AuditLogs_TenantId" ON public."AuditLogs" USING btree ("TenantId");


--
-- TOC entry 4955 (class 1259 OID 27725)
-- Name: IX_AuditLogs_TenantId_Action_EntityType; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_AuditLogs_TenantId_Action_EntityType" ON public."AuditLogs" USING btree ("TenantId", "Action", "EntityType");


--
-- TOC entry 4956 (class 1259 OID 27726)
-- Name: IX_AuditLogs_TenantId_CreatedAt; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_AuditLogs_TenantId_CreatedAt" ON public."AuditLogs" USING btree ("TenantId", "CreatedAt");


--
-- TOC entry 4959 (class 1259 OID 27727)
-- Name: IX_AutomationExecutions_RequestId; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_AutomationExecutions_RequestId" ON public."AutomationExecutions" USING btree ("RequestId");


--
-- TOC entry 4960 (class 1259 OID 27728)
-- Name: IX_AutomationExecutions_TenantId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_AutomationExecutions_TenantId" ON public."AutomationExecutions" USING btree ("TenantId");


--
-- TOC entry 4961 (class 1259 OID 27729)
-- Name: IX_AutomationExecutions_TenantId_Status; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_AutomationExecutions_TenantId_Status" ON public."AutomationExecutions" USING btree ("TenantId", "Status");


--
-- TOC entry 4924 (class 1259 OID 27532)
-- Name: IX_AutomationStates_CampaignId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_AutomationStates_CampaignId" ON public."AutomationStates" USING btree ("CampaignId");


--
-- TOC entry 4925 (class 1259 OID 27533)
-- Name: IX_AutomationStates_TenantId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_AutomationStates_TenantId" ON public."AutomationStates" USING btree ("TenantId");


--
-- TOC entry 4990 (class 1259 OID 27839)
-- Name: IX_CampaignDrafts_ConvertedCampaignId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_CampaignDrafts_ConvertedCampaignId" ON public."CampaignDrafts" USING btree ("ConvertedCampaignId");


--
-- TOC entry 4991 (class 1259 OID 27840)
-- Name: IX_CampaignDrafts_MarketingPackId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_CampaignDrafts_MarketingPackId" ON public."CampaignDrafts" USING btree ("MarketingPackId");


--
-- TOC entry 4992 (class 1259 OID 27841)
-- Name: IX_CampaignDrafts_TenantId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_CampaignDrafts_TenantId" ON public."CampaignDrafts" USING btree ("TenantId");


--
-- TOC entry 4993 (class 1259 OID 27842)
-- Name: IX_CampaignDrafts_TenantId_Status; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_CampaignDrafts_TenantId_Status" ON public."CampaignDrafts" USING btree ("TenantId", "Status");


--
-- TOC entry 5009 (class 1259 OID 27980)
-- Name: IX_CampaignMetrics_CampaignId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_CampaignMetrics_CampaignId" ON public."CampaignMetrics" USING btree ("CampaignId");


--
-- TOC entry 5010 (class 1259 OID 27981)
-- Name: IX_CampaignMetrics_TenantId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_CampaignMetrics_TenantId" ON public."CampaignMetrics" USING btree ("TenantId");


--
-- TOC entry 5011 (class 1259 OID 27982)
-- Name: IX_CampaignMetrics_TenantId_CampaignId_MetricDate; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_CampaignMetrics_TenantId_CampaignId_MetricDate" ON public."CampaignMetrics" USING btree ("TenantId", "CampaignId", "MetricDate");


--
-- TOC entry 5012 (class 1259 OID 27983)
-- Name: IX_CampaignMetrics_TenantId_MetricDate; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_CampaignMetrics_TenantId_MetricDate" ON public."CampaignMetrics" USING btree ("TenantId", "MetricDate");


--
-- TOC entry 4920 (class 1259 OID 27534)
-- Name: IX_Campaigns_TenantId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Campaigns_TenantId" ON public."Campaigns" USING btree ("TenantId");


--
-- TOC entry 4921 (class 1259 OID 27979)
-- Name: IX_Campaigns_TenantId_Status; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Campaigns_TenantId_Status" ON public."Campaigns" USING btree ("TenantId", "Status");


--
-- TOC entry 4936 (class 1259 OID 27535)
-- Name: IX_Consents_TenantId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Consents_TenantId" ON public."Consents" USING btree ("TenantId");


--
-- TOC entry 4937 (class 1259 OID 27536)
-- Name: IX_Consents_TenantId_UserId_ConsentType; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Consents_TenantId_UserId_ConsentType" ON public."Consents" USING btree ("TenantId", "UserId", "ConsentType");


--
-- TOC entry 4938 (class 1259 OID 27537)
-- Name: IX_Consents_UserId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Consents_UserId" ON public."Consents" USING btree ("UserId");


--
-- TOC entry 4928 (class 1259 OID 27538)
-- Name: IX_Contents_CampaignId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Contents_CampaignId" ON public."Contents" USING btree ("CampaignId");


--
-- TOC entry 4929 (class 1259 OID 27539)
-- Name: IX_Contents_TenantId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Contents_TenantId" ON public."Contents" USING btree ("TenantId");


--
-- TOC entry 4996 (class 1259 OID 27843)
-- Name: IX_GeneratedCopies_MarketingPackId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_GeneratedCopies_MarketingPackId" ON public."GeneratedCopies" USING btree ("MarketingPackId");


--
-- TOC entry 4997 (class 1259 OID 27844)
-- Name: IX_GeneratedCopies_TenantId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_GeneratedCopies_TenantId" ON public."GeneratedCopies" USING btree ("TenantId");


--
-- TOC entry 4998 (class 1259 OID 27845)
-- Name: IX_GeneratedCopies_TenantId_MarketingPackId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_GeneratedCopies_TenantId_MarketingPackId" ON public."GeneratedCopies" USING btree ("TenantId", "MarketingPackId");


--
-- TOC entry 5001 (class 1259 OID 27846)
-- Name: IX_MarketingAssetPrompts_MarketingPackId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_MarketingAssetPrompts_MarketingPackId" ON public."MarketingAssetPrompts" USING btree ("MarketingPackId");


--
-- TOC entry 5002 (class 1259 OID 27847)
-- Name: IX_MarketingAssetPrompts_TenantId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_MarketingAssetPrompts_TenantId" ON public."MarketingAssetPrompts" USING btree ("TenantId");


--
-- TOC entry 5003 (class 1259 OID 27848)
-- Name: IX_MarketingAssetPrompts_TenantId_MarketingPackId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_MarketingAssetPrompts_TenantId_MarketingPackId" ON public."MarketingAssetPrompts" USING btree ("TenantId", "MarketingPackId");


--
-- TOC entry 4932 (class 1259 OID 27540)
-- Name: IX_MarketingMemories_CampaignId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_MarketingMemories_CampaignId" ON public."MarketingMemories" USING btree ("CampaignId");


--
-- TOC entry 4933 (class 1259 OID 27541)
-- Name: IX_MarketingMemories_TenantId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_MarketingMemories_TenantId" ON public."MarketingMemories" USING btree ("TenantId");


--
-- TOC entry 4983 (class 1259 OID 27849)
-- Name: IX_MarketingPacks_CampaignId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_MarketingPacks_CampaignId" ON public."MarketingPacks" USING btree ("CampaignId");


--
-- TOC entry 4984 (class 1259 OID 27850)
-- Name: IX_MarketingPacks_ContentId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_MarketingPacks_ContentId" ON public."MarketingPacks" USING btree ("ContentId");


--
-- TOC entry 4985 (class 1259 OID 27851)
-- Name: IX_MarketingPacks_TenantId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_MarketingPacks_TenantId" ON public."MarketingPacks" USING btree ("TenantId");


--
-- TOC entry 4986 (class 1259 OID 27852)
-- Name: IX_MarketingPacks_TenantId_ContentId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_MarketingPacks_TenantId_ContentId" ON public."MarketingPacks" USING btree ("TenantId", "ContentId");


--
-- TOC entry 4987 (class 1259 OID 27853)
-- Name: IX_MarketingPacks_TenantId_Status; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_MarketingPacks_TenantId_Status" ON public."MarketingPacks" USING btree ("TenantId", "Status");


--
-- TOC entry 5024 (class 1259 OID 27984)
-- Name: IX_PublishingJobMetrics_PublishingJobId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PublishingJobMetrics_PublishingJobId" ON public."PublishingJobMetrics" USING btree ("PublishingJobId");


--
-- TOC entry 5025 (class 1259 OID 27985)
-- Name: IX_PublishingJobMetrics_TenantId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PublishingJobMetrics_TenantId" ON public."PublishingJobMetrics" USING btree ("TenantId");


--
-- TOC entry 5026 (class 1259 OID 27986)
-- Name: IX_PublishingJobMetrics_TenantId_MetricDate; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PublishingJobMetrics_TenantId_MetricDate" ON public."PublishingJobMetrics" USING btree ("TenantId", "MetricDate");


--
-- TOC entry 5027 (class 1259 OID 27987)
-- Name: IX_PublishingJobMetrics_TenantId_PublishingJobId_MetricDate; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_PublishingJobMetrics_TenantId_PublishingJobId_MetricDate" ON public."PublishingJobMetrics" USING btree ("TenantId", "PublishingJobId", "MetricDate");


--
-- TOC entry 5015 (class 1259 OID 27988)
-- Name: IX_PublishingJobs_CampaignId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PublishingJobs_CampaignId" ON public."PublishingJobs" USING btree ("CampaignId");


--
-- TOC entry 5016 (class 1259 OID 27989)
-- Name: IX_PublishingJobs_GeneratedCopyId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PublishingJobs_GeneratedCopyId" ON public."PublishingJobs" USING btree ("GeneratedCopyId");


--
-- TOC entry 5017 (class 1259 OID 27990)
-- Name: IX_PublishingJobs_MarketingAssetPromptId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PublishingJobs_MarketingAssetPromptId" ON public."PublishingJobs" USING btree ("MarketingAssetPromptId");


--
-- TOC entry 5018 (class 1259 OID 27991)
-- Name: IX_PublishingJobs_MarketingPackId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PublishingJobs_MarketingPackId" ON public."PublishingJobs" USING btree ("MarketingPackId");


--
-- TOC entry 5019 (class 1259 OID 27992)
-- Name: IX_PublishingJobs_TenantId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PublishingJobs_TenantId" ON public."PublishingJobs" USING btree ("TenantId");


--
-- TOC entry 5020 (class 1259 OID 27993)
-- Name: IX_PublishingJobs_TenantId_CampaignId_Status; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PublishingJobs_TenantId_CampaignId_Status" ON public."PublishingJobs" USING btree ("TenantId", "CampaignId", "Status");


--
-- TOC entry 5021 (class 1259 OID 27994)
-- Name: IX_PublishingJobs_TenantId_ScheduledDate; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PublishingJobs_TenantId_ScheduledDate" ON public."PublishingJobs" USING btree ("TenantId", "ScheduledDate");


--
-- TOC entry 5006 (class 1259 OID 27874)
-- Name: IX_TenantAIConfigs_TenantId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_TenantAIConfigs_TenantId" ON public."TenantAIConfigs" USING btree ("TenantId");


--
-- TOC entry 5030 (class 1259 OID 28033)
-- Name: IX_TenantN8nConfigs_TenantId; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_TenantN8nConfigs_TenantId" ON public."TenantN8nConfigs" USING btree ("TenantId");


--
-- TOC entry 4917 (class 1259 OID 27542)
-- Name: IX_Tenants_Subdomain; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Tenants_Subdomain" ON public."Tenants" USING btree ("Subdomain");


--
-- TOC entry 4941 (class 1259 OID 27543)
-- Name: IX_UserPreferences_TenantId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_UserPreferences_TenantId" ON public."UserPreferences" USING btree ("TenantId");


--
-- TOC entry 4942 (class 1259 OID 27544)
-- Name: IX_UserPreferences_TenantId_UserId_PreferenceKey; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_UserPreferences_TenantId_UserId_PreferenceKey" ON public."UserPreferences" USING btree ("TenantId", "UserId", "PreferenceKey");


--
-- TOC entry 4943 (class 1259 OID 27545)
-- Name: IX_UserPreferences_UserId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_UserPreferences_UserId" ON public."UserPreferences" USING btree ("UserId");


--
-- TOC entry 4978 (class 1259 OID 27730)
-- Name: IX_UserTenants_RoleId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_UserTenants_RoleId" ON public."UserTenants" USING btree ("RoleId");


--
-- TOC entry 4979 (class 1259 OID 27731)
-- Name: IX_UserTenants_TenantId_RoleId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_UserTenants_TenantId_RoleId" ON public."UserTenants" USING btree ("TenantId", "RoleId");


--
-- TOC entry 4980 (class 1259 OID 27732)
-- Name: IX_UserTenants_UserId_TenantId; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_UserTenants_UserId_TenantId" ON public."UserTenants" USING btree ("UserId", "TenantId");


--
-- TOC entry 4948 (class 1259 OID 27717)
-- Name: RoleNameIndex; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "RoleNameIndex" ON public."AspNetRoles" USING btree ("NormalizedName");


--
-- TOC entry 4953 (class 1259 OID 27723)
-- Name: UserNameIndex; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "UserNameIndex" ON public."AspNetUsers" USING btree ("NormalizedUserName");


--
-- TOC entry 5040 (class 2606 OID 27621)
-- Name: AspNetRoleClaims FK_AspNetRoleClaims_AspNetRoles_RoleId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetRoleClaims"
    ADD CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."AspNetRoles"("Id") ON DELETE CASCADE;


--
-- TOC entry 5041 (class 2606 OID 27636)
-- Name: AspNetUserClaims FK_AspNetUserClaims_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserClaims"
    ADD CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 5042 (class 2606 OID 27651)
-- Name: AspNetUserLogins FK_AspNetUserLogins_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserLogins"
    ADD CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 5043 (class 2606 OID 27663)
-- Name: AspNetUserRoles FK_AspNetUserRoles_AspNetRoles_RoleId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."AspNetRoles"("Id") ON DELETE CASCADE;


--
-- TOC entry 5044 (class 2606 OID 27668)
-- Name: AspNetUserRoles FK_AspNetUserRoles_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 5045 (class 2606 OID 27683)
-- Name: AspNetUserTokens FK_AspNetUserTokens_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserTokens"
    ADD CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 5039 (class 2606 OID 27576)
-- Name: AspNetUsers FK_AspNetUsers_Tenants_TenantId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUsers"
    ADD CONSTRAINT "FK_AspNetUsers_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES public."Tenants"("Id") ON DELETE RESTRICT;


--
-- TOC entry 5034 (class 2606 OID 27449)
-- Name: AutomationStates FK_AutomationStates_Campaigns_CampaignId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AutomationStates"
    ADD CONSTRAINT "FK_AutomationStates_Campaigns_CampaignId" FOREIGN KEY ("CampaignId") REFERENCES public."Campaigns"("Id") ON DELETE SET NULL;


--
-- TOC entry 5051 (class 2606 OID 27791)
-- Name: CampaignDrafts FK_CampaignDrafts_Campaigns_ConvertedCampaignId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."CampaignDrafts"
    ADD CONSTRAINT "FK_CampaignDrafts_Campaigns_ConvertedCampaignId" FOREIGN KEY ("ConvertedCampaignId") REFERENCES public."Campaigns"("Id") ON DELETE SET NULL;


--
-- TOC entry 5052 (class 2606 OID 27796)
-- Name: CampaignDrafts FK_CampaignDrafts_MarketingPacks_MarketingPackId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."CampaignDrafts"
    ADD CONSTRAINT "FK_CampaignDrafts_MarketingPacks_MarketingPackId" FOREIGN KEY ("MarketingPackId") REFERENCES public."MarketingPacks"("Id") ON DELETE RESTRICT;


--
-- TOC entry 5056 (class 2606 OID 27910)
-- Name: CampaignMetrics FK_CampaignMetrics_Campaigns_CampaignId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."CampaignMetrics"
    ADD CONSTRAINT "FK_CampaignMetrics_Campaigns_CampaignId" FOREIGN KEY ("CampaignId") REFERENCES public."Campaigns"("Id") ON DELETE RESTRICT;


--
-- TOC entry 5033 (class 2606 OID 27408)
-- Name: Campaigns FK_Campaigns_Tenants_TenantId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Campaigns"
    ADD CONSTRAINT "FK_Campaigns_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES public."Tenants"("Id") ON DELETE RESTRICT;


--
-- TOC entry 5037 (class 2606 OID 28001)
-- Name: Consents FK_Consents_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Consents"
    ADD CONSTRAINT "FK_Consents_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 5035 (class 2606 OID 27468)
-- Name: Contents FK_Contents_Campaigns_CampaignId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Contents"
    ADD CONSTRAINT "FK_Contents_Campaigns_CampaignId" FOREIGN KEY ("CampaignId") REFERENCES public."Campaigns"("Id") ON DELETE SET NULL;


--
-- TOC entry 5053 (class 2606 OID 27815)
-- Name: GeneratedCopies FK_GeneratedCopies_MarketingPacks_MarketingPackId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."GeneratedCopies"
    ADD CONSTRAINT "FK_GeneratedCopies_MarketingPacks_MarketingPackId" FOREIGN KEY ("MarketingPackId") REFERENCES public."MarketingPacks"("Id") ON DELETE CASCADE;


--
-- TOC entry 5054 (class 2606 OID 27834)
-- Name: MarketingAssetPrompts FK_MarketingAssetPrompts_MarketingPacks_MarketingPackId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."MarketingAssetPrompts"
    ADD CONSTRAINT "FK_MarketingAssetPrompts_MarketingPacks_MarketingPackId" FOREIGN KEY ("MarketingPackId") REFERENCES public."MarketingPacks"("Id") ON DELETE CASCADE;


--
-- TOC entry 5036 (class 2606 OID 27488)
-- Name: MarketingMemories FK_MarketingMemories_Campaigns_CampaignId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."MarketingMemories"
    ADD CONSTRAINT "FK_MarketingMemories_Campaigns_CampaignId" FOREIGN KEY ("CampaignId") REFERENCES public."Campaigns"("Id") ON DELETE SET NULL;


--
-- TOC entry 5049 (class 2606 OID 27765)
-- Name: MarketingPacks FK_MarketingPacks_Campaigns_CampaignId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."MarketingPacks"
    ADD CONSTRAINT "FK_MarketingPacks_Campaigns_CampaignId" FOREIGN KEY ("CampaignId") REFERENCES public."Campaigns"("Id") ON DELETE SET NULL;


--
-- TOC entry 5050 (class 2606 OID 27770)
-- Name: MarketingPacks FK_MarketingPacks_Contents_ContentId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."MarketingPacks"
    ADD CONSTRAINT "FK_MarketingPacks_Contents_ContentId" FOREIGN KEY ("ContentId") REFERENCES public."Contents"("Id") ON DELETE RESTRICT;


--
-- TOC entry 5061 (class 2606 OID 27973)
-- Name: PublishingJobMetrics FK_PublishingJobMetrics_PublishingJobs_PublishingJobId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PublishingJobMetrics"
    ADD CONSTRAINT "FK_PublishingJobMetrics_PublishingJobs_PublishingJobId" FOREIGN KEY ("PublishingJobId") REFERENCES public."PublishingJobs"("Id") ON DELETE RESTRICT;


--
-- TOC entry 5057 (class 2606 OID 27933)
-- Name: PublishingJobs FK_PublishingJobs_Campaigns_CampaignId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PublishingJobs"
    ADD CONSTRAINT "FK_PublishingJobs_Campaigns_CampaignId" FOREIGN KEY ("CampaignId") REFERENCES public."Campaigns"("Id") ON DELETE RESTRICT;


--
-- TOC entry 5058 (class 2606 OID 27938)
-- Name: PublishingJobs FK_PublishingJobs_GeneratedCopies_GeneratedCopyId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PublishingJobs"
    ADD CONSTRAINT "FK_PublishingJobs_GeneratedCopies_GeneratedCopyId" FOREIGN KEY ("GeneratedCopyId") REFERENCES public."GeneratedCopies"("Id") ON DELETE SET NULL;


--
-- TOC entry 5059 (class 2606 OID 27943)
-- Name: PublishingJobs FK_PublishingJobs_MarketingAssetPrompts_MarketingAssetPromptId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PublishingJobs"
    ADD CONSTRAINT "FK_PublishingJobs_MarketingAssetPrompts_MarketingAssetPromptId" FOREIGN KEY ("MarketingAssetPromptId") REFERENCES public."MarketingAssetPrompts"("Id") ON DELETE SET NULL;


--
-- TOC entry 5060 (class 2606 OID 27948)
-- Name: PublishingJobs FK_PublishingJobs_MarketingPacks_MarketingPackId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PublishingJobs"
    ADD CONSTRAINT "FK_PublishingJobs_MarketingPacks_MarketingPackId" FOREIGN KEY ("MarketingPackId") REFERENCES public."MarketingPacks"("Id") ON DELETE SET NULL;


--
-- TOC entry 5055 (class 2606 OID 27869)
-- Name: TenantAIConfigs FK_TenantAIConfigs_Tenants_TenantId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."TenantAIConfigs"
    ADD CONSTRAINT "FK_TenantAIConfigs_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES public."Tenants"("Id") ON DELETE CASCADE;


--
-- TOC entry 5062 (class 2606 OID 28028)
-- Name: TenantN8nConfigs FK_TenantN8nConfigs_Tenants_TenantId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."TenantN8nConfigs"
    ADD CONSTRAINT "FK_TenantN8nConfigs_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES public."Tenants"("Id") ON DELETE RESTRICT;


--
-- TOC entry 5038 (class 2606 OID 28006)
-- Name: UserPreferences FK_UserPreferences_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserPreferences"
    ADD CONSTRAINT "FK_UserPreferences_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 5046 (class 2606 OID 27701)
-- Name: UserTenants FK_UserTenants_AspNetRoles_RoleId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserTenants"
    ADD CONSTRAINT "FK_UserTenants_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."AspNetRoles"("Id") ON DELETE RESTRICT;


--
-- TOC entry 5047 (class 2606 OID 27706)
-- Name: UserTenants FK_UserTenants_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserTenants"
    ADD CONSTRAINT "FK_UserTenants_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 5048 (class 2606 OID 27711)
-- Name: UserTenants FK_UserTenants_Tenants_TenantId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserTenants"
    ADD CONSTRAINT "FK_UserTenants_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES public."Tenants"("Id") ON DELETE RESTRICT;


-- Completed on 2025-12-30 21:16:42

--
-- PostgreSQL database dump complete
--

\unrestrict Uq0tdwC3sF07syKaISeVFUtuObd6dLsQ7W5IRNdFzhVpv25Xu9vAkuRQJgaVyka

